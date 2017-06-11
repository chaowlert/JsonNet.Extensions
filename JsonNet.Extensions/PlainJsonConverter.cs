﻿using Newtonsoft.Json;
using System;

namespace JsonNet.Extensions
{
    public class PlainJsonConverter : JsonConverter
    {
        public static readonly PlainJsonConverter Default = new PlainJsonConverter();

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteRawValue((string)value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }
    }
}