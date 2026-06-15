using System;
using System.Numerics;
using Newtonsoft.Json;

namespace Data.Utils
{
    public class StringToBigIntegerConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new Exception();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var o = serializer.Deserialize<string>(reader);
            var sValue = o;

            if (!string.IsNullOrEmpty(sValue) &&
                BigInteger.TryParse(sValue, out var value))
            {
                return value;
            }

            return 0;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(BigInteger);
        }
    }
}