using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Hangfire;
using iGadget.DB;
using iGadget.DB.Models;
using iGadget.App.Models.Config;
using iGadget.App.Util;
using Newtonsoft.Json;
using UnitsNet;

namespace iGadget.App.Tasks.Weather
{
    public class NWSTasks
    {
        private const string NWSAccept = "application/geo+json,application/ld+json,application/json";

        private IServiceProvider _serviceProvider;

        private IConfiguration _config;

        public NWSTasks(IServiceProvider sp)
        {
            _serviceProvider = sp;
            _config = _serviceProvider.GetService<IConfiguration>();
        }

        [ObjectFriendlyJobDisplayName("Download Latest National Weather Service Forecast for {0:Name} ({0:Lat:0.####} N, {0:Lon:0.####} W)")]
        [AutomaticRetry(Attempts = 5, DelaysInSeconds = new int[] { 10, 90, 120, 240, 240 }, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public async Task GetWeatherForecast(Models.Config.NWSSite nwsSite)
        {
            var nwsApiServer = _config["NationalWeatherService:APIServer"];
            Uri forecastUrl = (await GetPoint(nwsSite.Lat, nwsSite.Lon)).Properties.Forecast;
            try
            {
                var forecastResponseString = await WebClient.GetString(forecastUrl, NWSAccept);
                var forecastResponse = JsonConvert.DeserializeObject<Models.Weather.NWSAPI.Forecast.NwsForecast>(forecastResponseString, Models.Weather.NWSAPI.Forecast.Converter.Settings);
                var forecasts = forecastResponse.Properties.Periods;

                var forecastPeriods = forecasts.OrderBy(a => a.Number).Select(a => new DB.Models.Forecast {
                    ValidDate = a.StartTime,
                    DetailedForecast = a.DetailedForecast,
                    IconURL = a.Icon,
                    IsDaytime = a.IsDaytime,
                    Name = a.Name,
                    Temperature = a.Temperature
                }).ToList();

                using (IServiceScope scope = _serviceProvider.CreateScope())
                using (var dbContext = scope.ServiceProvider.GetRequiredService<iGadgetDataCache>())
                {
                    var dbSite = dbContext.WeatherSites.Include(a => a.Forecasts).FirstOrDefault<WeatherSite>(a => a.Name == nwsSite.Name);
                    if (dbSite == null)
                    {
                        dbSite = new WeatherSite {
                            Name = nwsSite.Name,
                            Lat = nwsSite.Lat,
                            Lon = nwsSite.Lon,
                            URL = nwsSite.URL,
                            Forecasts = forecastPeriods
                        };
                        dbContext.WeatherSites.Add(dbSite);
                        dbContext.SaveChanges();
                    }
                    else
                    {
                        if (dbSite.Forecasts.Any())
                        {
                            if (dbSite.Forecasts.OrderBy(a => a.ValidDate).First().ValidDate == forecastPeriods[0].ValidDate)
                            {
                                return;
                            }
                            dbContext.RemoveRange(dbSite.Forecasts);
                            dbSite.Forecasts.ForEach(a => dbContext.Entry(a).State = EntityState.Deleted);
                            dbSite.Forecasts.Clear();
                            dbContext.SaveChanges();
                        }

                        dbContext.AddRange(forecastPeriods);
                        dbSite.Forecasts.AddRange(forecastPeriods);
                        dbSite.Forecasts.ForEach(a => dbContext.Entry(a).State = EntityState.Added);
                        dbContext.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Unable to parse response from NWS to extract forecast for {nwsSite.Name} ({nwsSite.Lat:0.####} N, {nwsSite.Lon:0.####} W).", e);
            }

        }

        [ObjectFriendlyJobDisplayName("Download Latest National Weather Service Observations for {0:Name} ({0:Lat:0.####} N, {0:Lon:0.####} W)")]
        [AutomaticRetry(Attempts = 2, DelaysInSeconds = new int[] { 10, 90, 120, 240, 240 }, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public async Task GetWeatherObservations(NWSSite nwsSite)
        {
            var nwsApiServer = _config["NationalWeatherService:APIServer"];
            using (IServiceScope scope = _serviceProvider.CreateScope())
            using (var dbContext = scope.ServiceProvider.GetRequiredService<iGadgetDataCache>())
            {
                var dbSite = dbContext.WeatherSites.FirstOrDefault<WeatherSite>(a => a.Name == nwsSite.Name);
                if (dbSite == null)
                {
                    dbSite = new WeatherSite {
                        Name = nwsSite.Name,
                        Lat = nwsSite.Lat,
                        Lon = nwsSite.Lon,
                        URL = nwsSite.URL,
                    };
                    dbContext.WeatherSites.Add(dbSite);
                    dbContext.SaveChanges();

                    dbSite = dbContext.WeatherSites.Include(a => a.Forecasts).FirstOrDefault<WeatherSite>(a => a.Name == nwsSite.Name);
                }

                Uri observationStationApiUrl = (await GetPoint(nwsSite.Lat, nwsSite.Lon)).Properties.ObservationStations;

                string stationIdentifier = "";
                try
                {
                    var stationContent = await WebClient.GetString(observationStationApiUrl, NWSAccept);
                    var stationApiResult = JsonConvert.DeserializeObject<Models.Weather.NWSAPI.Stations.NwsStations>(stationContent, Models.Weather.NWSAPI.Stations.Converter.Settings);
                    stationIdentifier = stationApiResult.Features[0].Properties.StationIdentifier;
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException($"Unable to parse response from NWS to extract observation station list for stations near {nwsSite.Name} ({nwsSite.Lat:0.####} N, {nwsSite.Lon:0.####} W)", e);
                }

                try
                {
                    var observationContent = await WebClient.GetString($"{nwsApiServer}/stations/{stationIdentifier}/observations/latest", NWSAccept);
                    var currentWeather = JsonConvert.DeserializeObject<Models.Weather.NWSAPI.Observation.NwsObservation>(observationContent, Models.Weather.NWSAPI.Observation.Converter.Settings);
                    var properties = currentWeather.Properties;

                    dbSite.TextDescription = properties.TextDescription;
                    dbSite.Icon = properties.Icon;
                    dbSite.Requested = DateTime.Now;
                    dbSite.LatestObservationTime = properties.Timestamp;
                    dbSite.Humidity = (long)(properties.RelativeHumidity.Value ?? -1);

                    if (properties.WindDirection.Value != null)
                    {
                        double bearing = (double)properties.WindDirection.Value;
                        dbSite.WindDirection = DegreesToCardinalDetailed(bearing);
                    }

                    if (properties.WindSpeed.Value != null)
                    {
                        double windSpeed = (double)properties.WindSpeed.Value;
                        string unitCode = properties.WindSpeed.UnitCode;
                        if (unitCode != null && unitCode.Contains("km_h"))
                        {
                            dbSite.WindSpeed = (long)Speed.FromKilometersPerHour(windSpeed).MilesPerHour;
                        }
                        else
                        {
                            dbSite.WindSpeed = (long)windSpeed;
                        }
                    }

                    if (properties.Temperature.Value != null)
                    {
                        double temp = (double)properties.Temperature.Value;
                        string unitCode = properties.Temperature.UnitCode;

                        if (unitCode != null && unitCode.Contains("degC"))
                        {
                            dbSite.Temp = (long)Temperature.FromDegreesCelsius(temp).DegreesFahrenheit;
                        }
                        else
                        {
                            dbSite.Temp = (long)temp;
                        }
                    }

                    dbContext.SaveChanges();
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException($"Unable to parse response from NWS to extract current observations for {nwsSite.Name} ({nwsSite.Lat:0.####} N, {nwsSite.Lon:0.####} W).", e);
                }
            }
        }

        private async Task<Models.Weather.NWSAPI.Point.NwsPoint> GetPoint(double lat, double lon)
        {
            try
            {
                var nwsApiServer = _config["NationalWeatherService:APIServer"];
                var pointContent = await WebClient.GetString($"{nwsApiServer}/points/{lat:0.####},{lon:0.####}", NWSAccept);
                var pointApiResult = JsonConvert.DeserializeObject<Models.Weather.NWSAPI.Point.NwsPoint>(pointContent, Models.Weather.NWSAPI.Point.Converter.Settings);
                return pointApiResult;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Unable to parse response from NWS to extract point data about ({lat:0.####} N, {lon:0.####} W)", e);
            }
        }

        private static string DegreesToCardinalDetailed(double degrees)
        {
            string[] cardinals = { "N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE", "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW", "N" };
            return cardinals[(int)Math.Round(((double)degrees * 10 % 3600) / 225)];
        }
    }
}