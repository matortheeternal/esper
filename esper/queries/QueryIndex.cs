#nullable enable

using esper.elements;
using esper.plugins;
using esper.resolution;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace esper.queries {
    public static class QueryIndex {
        static readonly string IndexDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "esper", "indexes"
        );

        public static Dictionary<uint, string> LoadOrBuild(
            PluginFile file,
            string recordSignature,
            string valueKey,
            Func<MainRecord, string> valueSelector
        ) {
            var path = IndexPath(file, recordSignature, valueKey);
            if (File.Exists(path))
                return Load(path);
            var index = Build(file, recordSignature, valueSelector);
            Save(path, index);
            return index;
        }

        static string IndexPath(PluginFile file, string recordSignature, string valueKey) {
            Directory.CreateDirectory(IndexDir);
            var md5 = FileMd5First8(file.filePath);
            var slug = Sanitize(valueKey);
            var name = $"{file.filename}-{md5}-{recordSignature}-{slug}.bin";
            return Path.Combine(IndexDir, name);
        }

        static string FileMd5First8(string path) {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(path);
            return Convert.ToHexString(md5.ComputeHash(stream))[..8].ToLowerInvariant();
        }

        static string Sanitize(string s) =>
            string.Join("_", s.Split(Path.GetInvalidFileNameChars()));

        internal static Dictionary<uint, string> Build(
            PluginFile file,
            string recordSignature,
            Func<MainRecord, string> valueSelector
        ) {
            return file.records
                .OfType<MainRecord>()
                .Where(r => r.signature.ToString() == recordSignature)
                .ToDictionary(r => r.fileFormId, valueSelector);
        }

        static string ResolveNameSignature(MainRecord record) {
            var referenced = record.ResolveElement("@NAME");
            return referenced is MainRecord refRec
                ? refRec.signature.ToString()
                : "";
        }

        static void Save(string path, Dictionary<uint, string> index) {
            using var fs = File.Create(path);
            using var bw = new BinaryWriter(fs, Encoding.UTF8, leaveOpen: true);
            bw.Write(index.Count);
            foreach (var kv in index.OrderBy(kv => kv.Key)) {
                bw.Write(kv.Key);
                var bytes = Encoding.UTF8.GetBytes(kv.Value);
                bw.Write(bytes);
                bw.Write((byte)0);
            }
        }

        static Dictionary<uint, string> Load(string path) {
            var index = new Dictionary<uint, string>();
            using var fs = File.OpenRead(path);
            using var br = new BinaryReader(fs, Encoding.UTF8, leaveOpen: true);
            var count = br.ReadInt32();
            for (int i = 0; i < count; i++) {
                var formId = br.ReadUInt32();
                var chars = new List<byte>();
                byte b;
                while ((b = br.ReadByte()) != 0)
                    chars.Add(b);
                index[formId] = Encoding.UTF8.GetString(chars.ToArray());
            }
            return index;
        }
    }
}
