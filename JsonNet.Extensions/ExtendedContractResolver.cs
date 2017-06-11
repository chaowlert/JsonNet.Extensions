using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JsonNet.Extensions
{
    public class ExtendedContractResolver : DefaultContractResolver
    {
        public List<JsonConverter> Converters { get; } = new List<JsonConverter>();
        public List<Func<JsonProperty, JsonProperty>> PropertyDecorators { get; } = new List<Func<JsonProperty, JsonProperty>>();

        protected override JsonContract CreateContract(Type objectType)
        {
            var contract = base.CreateContract(objectType);
            foreach (var converter in Converters)
            {
                if (converter.CanConvert(objectType))
                {
                    contract.Converter = converter;
                    break;
                }
            }
            return contract;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);

            return PropertyDecorators.Aggregate(prop, (current, decorator) => decorator(current));
        }
    }
}
