using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.IO;

namespace MoreDoors
{
    public static class JsonUtil
    {
        public static readonly JsonSerializer _js;

        public static T Deserialize<T>(string embeddedResourcePath)
        {
            using (StreamReader sr = new StreamReader(typeof(JsonUtil).Assembly.GetManifestResourceStream(embeddedResourcePath)))
            using (var jtr = new JsonTextReader(sr))
            {
                return _js.Deserialize<T>(jtr);
            }
        }

        static JsonUtil()
        {
            _js = new JsonSerializer
            {
                DefaultValueHandling = DefaultValueHandling.Include,
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto,
            };

            _js.Converters.Add(new StringEnumConverter());
        }
    }
}