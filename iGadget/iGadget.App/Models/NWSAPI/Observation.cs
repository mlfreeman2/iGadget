using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace iGadget.App.Models.Weather.NWSAPI.Observation
{
    public partial class NwsObservation
    {
        [JsonProperty("properties")]
        public Properties Properties { get; set; }
    }

    public partial class Properties
    {
        [JsonProperty("@id")]
        public Uri Id { get; set; }

        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("elevation")]
        public ValueAndUnit Elevation { get; set; }

        [JsonProperty("station")]
        public Uri Station { get; set; }

        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        [JsonProperty("rawMessage")]
        public string RawMessage { get; set; }

        [JsonProperty("textDescription")]
        public string TextDescription { get; set; }

        [JsonProperty("icon")]
        public Uri Icon { get; set; }

        [JsonProperty("presentWeather")]
        public object[] PresentWeather { get; set; }

        [JsonProperty("temperature")]
        public ValueAndUnit Temperature { get; set; }

        [JsonProperty("dewpoint")]
        public ValueAndUnit Dewpoint { get; set; }

        [JsonProperty("windDirection")]
        public ValueAndUnit WindDirection { get; set; }

        [JsonProperty("windSpeed")]
        public ValueAndUnit WindSpeed { get; set; }

        [JsonProperty("windGust")]
        public ValueAndUnit WindGust { get; set; }

        [JsonProperty("barometricPressure")]
        public ValueAndUnit BarometricPressure { get; set; }

        [JsonProperty("seaLevelPressure")]
        public ValueAndUnit SeaLevelPressure { get; set; }

        [JsonProperty("visibility")]
        public ValueAndUnit Visibility { get; set; }

        [JsonProperty("maxTemperatureLast24Hours")]
        public ValueAndUnit MaxTemperatureLast24Hours { get; set; }

        [JsonProperty("minTemperatureLast24Hours")]
        public ValueAndUnit MinTemperatureLast24Hours { get; set; }

        [JsonProperty("precipitationLastHour")]
        public ValueAndUnit PrecipitationLastHour { get; set; }

        [JsonProperty("precipitationLast3Hours")]
        public ValueAndUnit PrecipitationLast3Hours { get; set; }

        [JsonProperty("precipitationLast6Hours")]
        public ValueAndUnit PrecipitationLast6Hours { get; set; }

        [JsonProperty("relativeHumidity")]
        public ValueAndUnit RelativeHumidity { get; set; }

        [JsonProperty("windChill")]
        public ValueAndUnit WindChill { get; set; }

        [JsonProperty("heatIndex")]
        public ValueAndUnit HeatIndex { get; set; }

        [JsonProperty("cloudLayers")]
        public CloudLayer[] CloudLayers { get; set; }
    }

    public partial class ValueAndUnit
    {
        [JsonProperty("value")]
        public double? Value { get; set; }

        [JsonProperty("unitCode")]
        public string UnitCode { get; set; }

        [JsonProperty("qualityControl")]
        public string QualityControl { get; set; }
    }

    public partial class CloudLayer
    {
        [JsonProperty("base")]
        public ValueAndUnit Base { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }
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
