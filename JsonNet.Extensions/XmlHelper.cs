using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace JsonNet.Extensions
{
    public static class XmlHelper
    {
        public static JToken ToJson(this XDocument xDoc)
        {
            return xDoc.Root.ToJson();
        }

        static JObject ToJson(this IEnumerable<XAttribute> attributes)
        {
            if (!attributes.Any())
                return null;
            var json = new JObject();
            foreach (var attribute in attributes)
            {
                json.Add(attribute.Name.LocalName, attribute.Value);
            }
            return json;
        }

        static JToken ToJson(this IEnumerable<XElement> elements, JObject previous)
        {
            var group = elements.ToLookup(x => x.Name.LocalName);
            if (group.Count == 0)
                return previous;

            if (previous == null && group.Count == 1 && group.First().Key == "Item")
            {
                var one = group.First();
                if (one.Count() > 1 || one.First().Name.LocalName == "Item")
                    return one.ToJArray();
            }

            var result = previous ?? new JObject();
            foreach (var nodes in group)
            {
                var element = nodes.First();
                if (nodes.Count() == 1)
                    result.Add(element.Name.LocalName, element.ToJson());
                else
                    result.Add(element.Name.LocalName, nodes.ToJArray());
            }
            return result;
        }

        static JArray ToJArray(this IEnumerable<XElement> elements)
        {
            var array = new JArray();
            foreach (var element in elements)
            {
                array.Add(element.ToJson());
            }
            return array;
        }

        static JToken ToJson(this XElement xml)
        {
            var obj = xml.Attributes().ToJson();
            var result = xml.Elements().ToJson(obj);
            if (result == null)
                return new JValue(xml.Value);
            var jObject = result as JObject;
            if (jObject != null)
                jObject["Value"] = xml.Value;
            return result;
        }

        public static XDocument ToXml(this JToken json, bool noAttribute = false)
        {
            var result = json.ToXml("Root", noAttribute, 2);
            return new XDocument(result);
        }

        static XObject ToXml(this JToken json, string name, bool noAttribute, int noAttributeLevel = 0)
        {
            switch (json.Type)
            {
                case JTokenType.Object:
                    return ((JObject)json).ToXml(name, noAttribute, noAttributeLevel);
                case JTokenType.Array:
                    return ((JArray)json).ToXml(name, noAttribute);
                case JTokenType.Raw:
                    var str = ((JValue) json).Value?.ToString();
                    var json2 = str == null ? JValue.CreateNull() : JToken.Parse(str);
                    return json2.ToXml(name, noAttribute, noAttributeLevel);
                default:
                    return ((JValue)json).ToXml(name, noAttribute || noAttributeLevel > 0);
            }
        }

        static XElement ToXml(this JObject json, string name, bool noAttribute, int noAttributeLevel)
        {
            var list = new List<object>(json.Count);
            foreach (var prop in json)
            {
                list.Add(prop.Value.ToXml(prop.Key, noAttribute, noAttributeLevel - 1));
            }
            return new XElement(name, list.ToArray());
        }

        static XElement ToXml(this JArray json, string name, bool noAttribute)
        {
            var list = new List<object>(json.Count);
            foreach (var item in json)
            {
                list.Add(item.ToXml("Item", noAttribute, 1));
            }
            return new XElement(name, list.ToArray());
        }

        static XObject ToXml(this JValue json, string name, bool noAttribute)
        {
            if (noAttribute)
                return new XElement(name, (string)json);
            else
                return new XAttribute(name, (string)json);
        }
    }
}
