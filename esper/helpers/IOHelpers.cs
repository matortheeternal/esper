using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace esper.helpers {
    public static class IOHelpers {
        public static string ResolveResourcePath(string filename) {
            return Path.Join(Environment.CurrentDirectory, "data", filename);
        }

        public static JObject LoadResource(string filename) {
            var filePath = ResolveResourcePath(filename);
            return JObject.Parse(File.ReadAllText(filePath));
        }
    }
}