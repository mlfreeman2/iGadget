using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace iGadget.App.Models.Weather.NWSAPI.Stations
{
    public partial class NwsStations
    {
        [JsonProperty("features")]
        public Feature[] Features { get; set; }
    }

    public partial class Feature
    {
        [JsonProperty("id")]
        public Uri Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("geometry")]
        public Geometry Geometry { get; set; }

        [JsonProperty("properties")]
        public Properties Properties { get; set; }
    }

    public partial class Geometry
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("coordinates")]
        public double[] Coordinates { get; set; }
    }

    public partial class Properties
    {
        [JsonProperty("@id")]
        public Uri Id { get; set; }

        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("elevation")]
        public Elevation Elevation { get; set; }

        [JsonProperty("stationIdentifier")]
        public string StationIdentifier { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("timeZone")]
        public string TimeZone { get; set; }

        [JsonProperty("forecast")]
        public Uri Forecast { get; set; }

        [JsonProperty("county")]
        public Uri County { get; set; }

        [JsonProperty("fireWeatherZone")]
        public Uri FireWeatherZone { get; set; }
    }

    public partial class Elevation
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