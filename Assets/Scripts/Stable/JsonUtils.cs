
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

public static class JsonUtils {
    static JsonUtils() {
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings {
            Formatting = Formatting.Indented,
            Converters = new List<JsonConverter> { new StringEnumConverter() },
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            TypeNameHandling = TypeNameHandling.Auto
        };
    }
    public static string ToJson(this object value) {
        return JsonConvert.SerializeObject(value);
    }
    public static T FromJson<T>(this string json) {
        return JsonConvert.DeserializeObject<T>(json);
    }
}