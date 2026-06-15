#nullable enable
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Data.Utils
{
    public class DbFieldConverter: JsonConverter<DbField>
    {
        public override void WriteJson(JsonWriter writer, DbField? value, JsonSerializer serializer)
        {
            writer.WriteRawValue(value?.GetValue());
        }
    
        public override DbField ReadJson(JsonReader reader, Type objectType, DbField? existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            throw new Exception();
        }
    }
    // public class DbFieldConverter: JsonConverter<DbField>
    // {
    //     public override void WriteJson(JsonWriter writer, DbField? value, JsonSerializer serializer)
    //     {
    //         writer.WriteRawValue(value?.GetValue());
    //     }
    //
    //     public override DbField? ReadJson(JsonReader reader, Type objectType, DbField? existingValue, bool hasExistingValue,
    //         JsonSerializer serializer)
    //     {
    //         throw new NotImplementedException();
    //     }
    //
    //     public override bool CanRead => false;
    // }
}