using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.IO.Compression;

namespace esper.helpers {
    public static class IOHelpers {
        private static ZipArchive LoadDefinitionsArchive() {
            var definitionsPath = Path.Join(Environment.CurrentDirectory, "definitions.zip");
            var stream = File.OpenRead(definitionsPath);
            return new ZipArchive(stream, ZipArchiveMode.Read);
        }

        internal static JObject LoadDefinitions(string filename) {
            var archive = LoadDefinitionsArchive();
            var entry = archive.GetEntry(filename);
            var reader = new StreamReader(entry.Open());
            return JObject.Load(new JsonTextReader(reader));
        }

        internal static void SaveDefinitions(string filename, string definitions) {
            var outputPath = Path.Join(Environment.CurrentDirectory, filename);
            File.WriteAllText(outputPath, definitions);
        }
    }
}