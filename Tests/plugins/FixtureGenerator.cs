using esper;
using esper.data;
using esper.defs;
using esper.elements;
using esper.plugins;
using esper.setup;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Tests.plugins {
    [TestFixture]
    public class FixtureGenerator {
        private static string RepoRoot() {
            var dir = new DirectoryInfo(Environment.CurrentDirectory);
            while (dir != null && !Directory.Exists(Path.Join(dir.FullName, ".git")))
                dir = dir.Parent;
            return dir.FullName;
        }

        private class Selection {
            public Game game;
            public string master;
            public string fixture;
            public uint[] formIds;
        }

        private static readonly Selection[] selections = new Selection[] {
            new Selection {
                game = Games.SSE, master = "Skyrim.esm",
                fixture = "formats-sse.esp",
                formIds = new uint[] {
                    0x1B, 0xDD7, 0x12DFE, 0x5820A, 0x15CC2, 0x1BAEC,
                    0xE40, 0xEB9, 0x15F00, 0x1548E, 0x13126, 0xD1444,
                    0x15D24, 0x5F596, 0x46EF0, 0xD6989, 0x954E1,
                },
            },
            new Selection {
                game = Games.TES4, master = "Oblivion.esm",
                fixture = "formats-tes4.esp",
                formIds = new uint[] {
                    0x856, 0x8B5, 0x8BA, 0x8C9, 0x8CA, 0x18C5,
                    0x8F4, 0x15E, 0x12C, 0x9CA, 0x136,
                    0x2F86B, 0x23DD9, 0x30139, 0x2D078,
                },
            },
            new Selection {
                game = Games.FNV, master = "FalloutNV.esm",
                fixture = "formats-fnv.esp",
                formIds = new uint[] {
                    0xE58D5, 0xEFA4D, 0xF5266, 0x54E5F, 0x89466,
                    0x8E1C0, 0x8D0A2, 0xFFAE4, 0x107775, 0x152325,
                    0x31DAB, 0x31DAD,             0x24D5C, 0x30FEB,
            0xDA115, 0xEFB3B, 0xE790F, 0x371D1,
            0x14E8AD, 0x14E8BD, 0x152437,             0xEDB37, 0x302CF, 0x37138,
                    0x302CE,
                },
            },
            new Selection {
                game = Games.FO4, master = "Fallout4.esm",
                fixture = "formats-fo4.esp",
                formIds = new uint[] {
                    0xA042C, 0x20BD1, 0x1D394, 0x1D395, 0x1F986,
                    0x4A834, 0x58594, 0x72F3D, 0x8393, 0x83B1,
                    0x7, 0x19, 0x13746,
                },
            },
        };

        private static byte[] ReadRecordBytes(
            string masterPath, MainRecord rec, int headerSize
        ) {
            using var fs = File.OpenRead(masterPath);
            var len = (int)rec.header.dataSize + headerSize;
            var buf = new byte[len];
            fs.Seek(rec.bodyOffset - headerSize, SeekOrigin.Begin);
            var read = 0;
            while (read < len)
                read += fs.Read(buf, read, len - read);
            return buf;
        }

        private static byte[] Tes4Record(
            Game game, string masterPath, MainRecord masterHeader,
            int recordCount, uint nextId
        ) {
            var headerSize = (int)game.recordHeaderSize;
            using var fs = File.OpenRead(masterPath);
            var probe = new byte[headerSize + 10];
            fs.Seek(masterHeader.bodyOffset - headerSize, SeekOrigin.Begin);
            fs.Read(probe, 0, probe.Length);
            var version = BitConverter.ToSingle(probe, headerSize + 6);
            using var ms = new MemoryStream();
            using var w = new BinaryWriter(ms);
            var name = "Fixture" + game.abbreviation;
            var dataLen = 4 + 2 + 12 + 4 + 2 + (name.Length + 1);
            w.Write(Encoding.ASCII.GetBytes("TES4"));
            w.Write((uint)dataLen);
            w.Write(BitConverter.ToUInt32(probe, 8));
            w.Write((uint)0);
            if (headerSize > 20) w.Write((ulong)0);
            else w.Write((uint)0);
            w.Write(Encoding.ASCII.GetBytes("HEDR"));
            w.Write((ushort)12);
            w.Write(version);
            w.Write(recordCount);
            w.Write(nextId);
            w.Write(Encoding.ASCII.GetBytes("CNAM"));
            w.Write((ushort)(name.Length + 1));
            w.Write(Encoding.ASCII.GetBytes(name));
            w.Write((byte)0);
            return ms.ToArray();
        }

        private static byte[] GroupHeader(
            int headerSize, int groupType, byte[] label, int size
        ) {
            using var ms = new MemoryStream();
            using var w = new BinaryWriter(ms);
            w.Write(Encoding.ASCII.GetBytes("GRUP"));
            w.Write(size);
            w.Write(label);
            w.Write(groupType);
            w.Write(0);
            if (headerSize > 20) w.Write(0);
            return ms.ToArray();
        }

        private static void CollectStrings(Element el, HashSet<uint> ids) {
            if (el is ValueElement ve && ve.data is LocalizedString ls)
                ids.Add(ls.id);
            if (el is Container c) {
                List<Element> children;
                try {
                    children = c.elements;
                } catch (NotImplementedException) {
                    TestContext.Progress.WriteLine(
                        $"sort-key gap under {el.def.name}");
                    return;
                }
                if (children != null)
                    foreach (var child in children)
                        CollectStrings(child, ids);
            }
        }

        private static void WriteStrings(
            string path, Dictionary<uint, string> table, bool withPrefix
        ) {
            var ordered = table.OrderBy(kv => kv.Key).ToArray();
            var blobs = ordered.Select(kv => {
                var bytes = Encoding.Latin1.GetBytes(kv.Value);
                using var blob = new MemoryStream();
                using var bw = new BinaryWriter(blob);
                if (withPrefix) bw.Write((uint)(bytes.Length + 1));
                bw.Write(bytes);
                bw.Write((byte)0);
                return blob.ToArray();
            }).ToArray();
            using var ms = new MemoryStream();
            using var w = new BinaryWriter(ms);
            w.Write(ordered.Length);
            var dataSizePos = (int)ms.Position;
            w.Write(0);
            var offset = 0;
            for (var i = 0; i < ordered.Length; i++) {
                w.Write(ordered[i].Key);
                w.Write(offset);
                offset += blobs[i].Length;
            }
            var dataSize = offset;
            foreach (var blob in blobs) w.Write(blob);
            ms.Seek(dataSizePos, SeekOrigin.Begin);
            w.Write(dataSize);
            File.WriteAllBytes(path, ms.ToArray());
        }

        private static void LoadMasterStrings(
            Session session, PluginFile plugin, string master
        ) {
            var baseName = Path.GetFileNameWithoutExtension(master)
                .ToLowerInvariant();
            var dir = TestHelpers.FixturePath(baseName + "_strings");
            if (!Directory.Exists(dir)) return;
            var exts = new[] { ".strings", ".dlstrings", ".ilstrings" };
            var paths = exts
                .Select(ext => Path.Join(dir, baseName + "_english" + ext))
                .Where(File.Exists).ToList();
            if (paths.Count == 0) return;
            try {
                session.resourceManager.LoadPluginStrings(plugin, paths);
            } catch (Exception e) {
                TestContext.Progress.WriteLine(
                    $"master strings skipped: {e.GetType().Name}");
            }
        }

        private class GenTop {
            public string sig;
            public List<object> children = new List<object>();
            public Dictionary<string, int> keys =
                new Dictionary<string, int>();
        }

        private class GenRec {
            public byte[] raw;
            public List<object> children = new List<object>();
            public Dictionary<string, int> keys =
                new Dictionary<string, int>();
        }

        private class GenGroup {
            public int type;
            public byte[] label;
            public List<object> children = new List<object>();
            public Dictionary<string, int> keys =
                new Dictionary<string, int>();
        }

        private static void EmitKid(
            MemoryStream ms, object kid, int headerSize
        ) {
            if (kid is GenRec r) {
                ms.Write(r.raw, 0, r.raw.Length);
                foreach (var c in r.children) EmitKid(ms, c, headerSize);
                return;
            }
            var g = (GenGroup)kid;
            using var inner = new MemoryStream();
            foreach (var c in g.children.OfType<GenRec>())
                EmitKid(inner, c, headerSize);
            foreach (var c in g.children.OfType<GenGroup>())
                EmitKid(inner, c, headerSize);
            var payload = inner.ToArray();
            var gh = GroupHeader(
                headerSize, g.type, g.label,
                headerSize + payload.Length);
            ms.Write(gh, 0, gh.Length);
            ms.Write(payload, 0, payload.Length);
        }

        private static void Descend(
            List<object> kids, Dictionary<string, int> keys, string key,
            object node, out List<object> nextKids,
            out Dictionary<string, int> nextKeys
        ) {
            if (!keys.TryGetValue(key, out var idx)) {
                idx = kids.Count;
                kids.Add(node);
                keys[key] = idx;
            }
            var kid = kids[idx];
            if (kid is GenGroup gg) {
                nextKids = gg.children;
                nextKeys = gg.keys;
            } else {
                var gr = (GenRec)kid;
                nextKids = gr.children;
                nextKeys = gr.keys;
            }
        }

        private static void Generate(Selection sel, string fixturesDir) {
            var session = new Session(sel.game, new SessionOptions());
            var masterPath = TestHelpers.FixturePath(sel.master);
            if (!File.Exists(masterPath))
                Assert.Ignore($"{sel.master} missing.");
            var plugin = session.pluginManager.LoadPlugin(masterPath);
            LoadMasterStrings(session, plugin, sel.master);
            var headerSize = (int)sel.game.recordHeaderSize;
            var wanted = new HashSet<uint>(sel.formIds);
            var found = new List<MainRecord>();
            foreach (var rec in plugin.records) {
                if (!wanted.Contains(rec.fileFormId & 0xFFFFFFu)) continue;
                found.Add(rec);
            }
            Assert.AreEqual(
                wanted.Count, found.Count,
                $"missing records in {sel.master}");
            var outPath = Path.Join(fixturesDir, sel.fixture);
            using var ms = new MemoryStream();
            var topGroups = new Dictionary<string, GenTop>();
            var rawCache = new Dictionary<string, byte[]>();
            var placedRecs = new HashSet<string>();
            void EnsureGroupNode(
                GroupRecord gr, out List<object> gkids,
                out Dictionary<string, int> gkeys) {
                var c = gr.container;
                List<object> pkids;
                Dictionary<string, int> pkeys;
                if (c is GroupRecord pgr &&
                    pgr.groupDef is TopGroupDef) {
                    var tsig =
                        Encoding.ASCII.GetString(pgr.header.label);
                    if (!topGroups.TryGetValue(tsig, out var ttop)) {
                        ttop = new GenTop { sig = tsig };
                        topGroups[tsig] = ttop;
                    }
                    pkids = ttop.children;
                    pkeys = ttop.keys;
                } else if (c is GroupRecord pgr2) {
                    EnsureGroupNode(pgr2, out pkids, out pkeys);
                } else if (c is MainRecord pmr) {
                    EnsureRecordNode(pmr);
                    var pg = ParentGroupNode(pmr);
                    pkids = pg.kids;
                    pkeys = pg.keys;
                } else {
                    throw new Exception("Group has no parent.");
                }
                var key = "G" + gr.header.groupType +
                    Convert.ToBase64String(gr.header.label);
                Descend(pkids, pkeys, key, new GenGroup {
                    type = gr.header.groupType,
                    label = gr.header.label
                }, out gkids, out gkeys);
            }
            (List<object> kids, Dictionary<string, int> keys)
            ParentGroupNode(MainRecord r) {
                var c = r.container;
                if (c is GroupRecord g &&
                    g.groupDef is TopGroupDef) {
                    var tsig =
                        Encoding.ASCII.GetString(g.header.label);
                    if (!topGroups.TryGetValue(tsig, out var top)) {
                        top = new GenTop { sig = tsig };
                        topGroups[tsig] = top;
                    }
                    return (top.children, top.keys);
                }
                if (c is GroupRecord g2) {
                    EnsureGroupNode(g2, out var kids, out var keys);
                    return (kids, keys);
                }
                throw new Exception("Record has no group parent.");
            }
            void EnsureRecordNode(MainRecord r) {
                var key = "R" + r.signature +
                    (r.fileFormId & 0xFFFFFFu);
                if (!placedRecs.Add(key)) return;
                if (!rawCache.TryGetValue(key, out var rb)) {
                    rb = ReadRecordBytes(masterPath, r, headerSize);
                    rawCache[key] = rb;
                }
                var pg = ParentGroupNode(r);
                Descend(pg.kids, pg.keys, key, new GenRec { raw = rb },
                    out _, out _);
            }
            var maxId = 0u;
            var stringIds = new HashSet<uint>();
            foreach (var rec in found) {
                maxId = Math.Max(maxId, rec.fileFormId & 0xFFFFFFu);
                try {
                    var _ = rec.elements;
                    CollectStrings(rec, stringIds);
                } finally {
                    rec._internalElements = null;
                    rec._unexpectedSubrecords = null;
                }
                EnsureRecordNode(rec);
                Element e = rec;
                while (true) {
                    var c = e.container;
                    if (c is GroupRecord g) {
                        if (g.groupDef is TopGroupDef) break;
                        var p = g.GetParentRecord();
                        if (p != null) EnsureRecordNode(p);
                        e = g;
                    } else if (c is MainRecord mr) {
                        e = mr;
                    } else {
                        throw new Exception("No group parent.");
                    }
                }
            }
            using var w = new BinaryWriter(ms);
            var tes4 = Tes4Record(
                sel.game, masterPath, plugin.header, found.Count, maxId + 1);
            w.Write(tes4);
            foreach (var kv in topGroups.OrderBy(kv => kv.Key)) {
                using var pm = new MemoryStream();
                foreach (var kid in kv.Value.children.OfType<GenRec>())
                    EmitKid(pm, kid, headerSize);
                foreach (var kid in kv.Value.children.OfType<GenGroup>())
                    EmitKid(pm, kid, headerSize);
                var payload = pm.ToArray();
                w.Write(GroupHeader(
                    headerSize, 0, Encoding.ASCII.GetBytes(kv.Key),
                    headerSize + payload.Length));
                w.Write(payload);
            }
            File.WriteAllBytes(outPath, ms.ToArray());
            TestContext.Progress.WriteLine(
                $"wrote {outPath} ({found.Count} records)");
            var tables = new Dictionary<string, Dictionary<uint, string>>();
            if (plugin.stringFiles != null)
            foreach (var sf in plugin.stringFiles) {
                if (sf.Value == null) continue;
                foreach (var kv in sf.Value.strings) {
                    if (!stringIds.Contains(kv.Key)) continue;
                    var key = sf.Key.ToString();
                    if (!tables.TryGetValue(key, out var t)) {
                        t = new Dictionary<uint, string>();
                        tables[key] = t;
                    }
                    t[kv.Key] = kv.Value;
                }
            }
            foreach (var kv in tables) {
                var ext = kv.Key == "DLSTRINGS" ? ".dlstrings"
                    : kv.Key == "ILSTRINGS" ? ".ilstrings" : ".strings";
                var sp = Path.ChangeExtension(outPath, null) +
                    "_english" + ext;
                WriteStrings(
                    sp, kv.Value,
                    kv.Key == "DLSTRINGS" || kv.Key == "ILSTRINGS"
                );
                TestContext.Progress.WriteLine($"wrote {sp}");
            }
        }

        [Test]
        public void GenerateFormatFixtures() {
            if (Environment.GetEnvironmentVariable(
                    "ESPER_GENERATE_FIXTURES") != "1")
                Assert.Ignore("fixture generation not requested.");
            var fixturesDir = Path.Join(RepoRoot(), "fixtures");
            foreach (var sel in selections) Generate(sel, fixturesDir);
        }
    }
}
