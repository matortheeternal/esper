using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.IO.Compression;

namespace esper.helpers {
    public static class IOHelpers {
        public static JObject LoadDefinitions(string filename) {
            var definitionsPath = Path.Join(Environment.CurrentDirectory, "definitions.zip");
            var stream = File.OpenRead(definitionsPath);
            var archive = new ZipArchive(stream, ZipArchiveMode.Read);
            var entry = archive.GetEntry(filename);
            var reader = new StreamReader(entry.Open());
            return JObject.Parse(reader.ReadToEnd());
        }
    }
}