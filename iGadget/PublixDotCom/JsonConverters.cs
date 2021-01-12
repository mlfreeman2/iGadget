using System;
using System.Globalization;
using Newtonsoft.Json;

namespace PublixDotCom
{
    internal class StringToIntConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(int) || t == typeof(int?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) 
            {
                return null;
            }
            var value = serializer.Deserialize<string>(reader);
            int l;
            if (int.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type int");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (int)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly StringToIntConverter Singleton = new StringToIntConverter();
    }

    internal class StringToLongConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) 
            {
                return null;
            }
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (long.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly StringToLongConverter Singleton = new StringToLongConverter();
    }

    internal class StringToFloatConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(float) || t == typeof(float?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) 
            {
                return null;
            }
            var value = serializer.Deserialize<string>(reader);
            float l;
            if (float.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type float");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (float)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly StringToLongConverter Singleton = new StringToLongConverter();
    }

    internal class PublixDateConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(DateTime) || t == typeof(DateTime?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) 
            {
                return null;
            }
            var value = serializer.Deserialize<string>(reader);
            if (DateTime.TryParseExact(value, "MMM dd, yyyy hh:mm:ss tt", new CultureInfo("en-US"), DateTimeStyles.None, out DateTime l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type DateTime");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (DateTime)untypedValue;
            serializer.Serialize(writer, value.ToString("MMM dd, yyyy hh:mm:ss tt"));
            return;
        }

        public static readonly StringToLongConverter Singleton = new StringToLongConverter();
    }


}
