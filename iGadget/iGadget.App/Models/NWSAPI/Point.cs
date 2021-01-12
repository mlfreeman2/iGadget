using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace iGadget.App.Models.Weather.NWSAPI.Point
{
    public partial class NwsPoint
    {
        [JsonProperty("properties")]
        public NwsPointProperties Properties { get; set; }
    }

    public partial class NwsPointProperties
    {
        [JsonProperty("@id")]
        public Uri Id { get; set; }

        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("cwa")]
        public string Cwa { get; set; }

        [JsonProperty("forecastOffice")]
        public Uri ForecastOffice { get; set; }

        [JsonProperty("gridId")]
        public string GridId { get; set; }

        [JsonProperty("gridX")]
        public long GridX { get; set; }

        [JsonProperty("gridY")]
        public long GridY { get; set; }

        [JsonProperty("forecast")]
        public Uri Forecast { get; set; }

        [JsonProperty("forecastHourly")]
        public Uri ForecastHourly { get; set; }

        [JsonProperty("forecastGridData")]
        public Uri ForecastGridData { get; set; }

        [JsonProperty("observationStations")]
        public Uri ObservationStations { get; set; }

        [JsonProperty("relativeLocation")]
        public RelativeLocation RelativeLocation { get; set; }

        [JsonProperty("forecastZone")]
        public Uri ForecastZone { get; set; }

        [JsonProperty("county")]
        public Uri County { get; set; }

        [JsonProperty("fireWeatherZone")]
        public Uri FireWeatherZone { get; set; }

        [JsonProperty("timeZone")]
        public string TimeZone { get; set; }

        [JsonProperty("radarStation")]
        public string RadarStation { get; set; }
    }
    
    public partial class Geometry
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("coordinates")]
        public double[] Coordinates { get; set; }
    }

    public partial class RelativeLocation
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("geometry")]
        public Geometry Geometry { get; set; }

        [JsonProperty("properties")]
        public RelativeLocationProperties Properties { get; set; }
    }

    public partial class RelativeLocationProperties
    {
        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("distance")]
        public ValueAndUnit Distance { get; set; }

        [JsonProperty("bearing")]
        public ValueAndUnit Bearing { get; set; }
    }

    public partial class ValueAndUnit
    {
        [JsonProperty("value")]
        public double? Value { get; set; }

        [JsonProperty("unitCode")]
        public string UnitCode { get; set; }
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = { new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal } },
        };
    }
}
