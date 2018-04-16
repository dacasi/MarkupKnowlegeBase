using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.IO;

namespace KnowlegeBase.Core
{
    public static class JsonHelper
    {
        private readonly static JsonSerializerSettings DefaultSettings = new JsonSerializerSettings()
        {
            Culture = System.Globalization.CultureInfo.InvariantCulture,
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Objects,
            NullValueHandling = NullValueHandling.Ignore
        };

        public static string Serialize(object a_object, JsonSerializerSettings a_settings = null)
        {
            var settings = a_settings ?? DefaultSettings;
            return JsonConvert.SerializeObject(a_object, settings);
        }

        public static void SerializeToFile(string a_path, object a_object, JsonSerializerSettings a_settings = null)
        {
            File.WriteAllText(a_path, Serialize(a_object, a_settings), Encoding.UTF8);
        }

        public static T Deserialize<T>(string a_json, JsonSerializerSettings a_settings = null)
        {
            var settings = a_settings ?? DefaultSettings;
            return JsonConvert.DeserializeObject<T>(a_json, a_settings);
        }

        public static T DeserializeFromFile<T>(string a_path, JsonSerializerSettings a_settings = null)
        {
            return Deserialize<T>(File.ReadAllText(a_path, Encoding.UTF8), a_settings);
        }
    }
}
