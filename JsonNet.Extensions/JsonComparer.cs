using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace JsonNet.Extensions
{
    public class JsonComparer
    {
        public List<ChangeItem> ChangeItems { get; } = new List<ChangeItem>();

        public bool Visit(JToken left, JToken right, string prefix = "")
        {
            if (left == null && right == null)
                return false;

            var type = (left ?? right).Type;
            if (right != null && type != right.Type)
                type = JTokenType.None;
            else if (prefix != "" && (left == null || right == null))
                type = JTokenType.None;

            switch (type)
            {
                case JTokenType.Object:
                    return VisitObject((JObject)left, (JObject)right, prefix);
                case JTokenType.Array:
                    return VisitArray((JArray)left, (JArray)right, prefix);
                default:
                    return VisitToken(left, right, prefix);
            }
        }

        public static List<ChangeItem> Compare(JToken left, JToken right)
        {
            var comparer = new JsonComparer();
            comparer.Visit(left, right);
            return comparer.ChangeItems;
        }

        private bool VisitObject(JObject left, JObject right, string prefix)
        {
            var leftNames = new HashSet<string>();
            bool change = false;

            if (left != null)
            {
                foreach (var kvp in left)
                {
                    leftNames.Add(kvp.Key);
                    var key = string.IsNullOrEmpty(prefix) ? kvp.Key : prefix + '.' + kvp.Key;
                    var valueChange = Visit(kvp.Value, right?[kvp.Key], key);
                    if (!valueChange)
                        continue;

                    change = true;
                }
            }
            if (right != null)
            {
                foreach (var kvp in right)
                {
                    if (leftNames.Contains(kvp.Key))
                        continue;

                    var key = string.IsNullOrEmpty(prefix) ? kvp.Key : prefix + '.' + kvp.Key;
                    var valueChange = Visit(null, kvp.Value, key);
                    if (!valueChange)
                        continue;

                    change = true;
                }
            }

            return change;
        }

        private bool VisitArray(JArray left, JArray right, string prefix)
        {
            var leftLen = left?.Count ?? 0;
            var rightLen = right?.Count ?? 0;
            var len = Math.Max(leftLen, rightLen);
            bool change = false;

            for (var i = 0; i < len; i++)
            {
                var leftItem = i < leftLen ? left?[i] : null;
                var rightItem = i < rightLen ? right?[i] : null;
                var valueChange = Visit(leftItem, rightItem, $"{prefix}[{i}]");
                if (!valueChange)
                    continue;

                change = true;
            }

            return change;
        }

        private bool VisitToken(JToken left, JToken right, string prefix)
        {
            var equal = JToken.DeepEquals(left, right);
            if (equal)
                return false;
            ChangeItems.Add(new ChangeItem
            {
                Path = prefix,
                Remove = left?.ToString(),
                Add = right?.ToString(),
            });
            return true;
        }
    }

    public class ChangeItem
    {
        public string Path { get; set; }
        public string Add { get; set; }
        public string Remove { get; set; }
    }
}
