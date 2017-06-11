using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;

public static class JsonNetExtensions
{
    static JsonSerializerSettings _settings = new JsonSerializerSettings();
    static JsonSerializer _serializer = JsonSerializer.CreateDefault(_settings);
    public static JsonSerializerSettings Settings
    {
        get { return _settings; }
        set
        {
            _settings = value;
            _serializer = JsonSerializer.CreateDefault(_settings);
        }
    }

    public static T JsonDeserialize<T>(this string value)
    {
        return JsonConvert.DeserializeObject<T>(value, _settings);
    }

    public static string JsonSerialize<T>(this T value)
    {
        return JsonConvert.SerializeObject(value, _settings);
    }

    public static JObject ToJObject(this object value)
    {
        return JObject.FromObject(value, _serializer);
    }

    public static JArray ToJArray(this IEnumerable value)
    {
        return JArray.FromObject(value, _serializer);
    }

    public static T JsonDeserialize<T>(this Stream stream)
    {
        using (var sr = new StreamReader(stream))
        using (var reader = new JsonTextReader(sr))
        {
            return _serializer.Deserialize<T>(reader);
        }
    }

    public static JArray ReadToJArray(this DbDataReader reader)
    {
        var results = new List<Dictionary<string, object>>();
        var cols = new List<string>();
        for (var i = 0; i < reader.FieldCount; i++)
            cols.Add(reader.GetName(i));

        while (reader.Read())
            results.Add(cols.ToDictionary(col => col, col => reader[col]));

        return results.ToJArray();
    }
}
