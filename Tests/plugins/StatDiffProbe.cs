using System;
using System.IO;
using esper;
using esper.defs;
using esper.elements;
using esper.plugins;
using esper.resolution;
using esper.setup;
using NUnit.Framework;

namespace Tests.plugins {
    [TestFixture]
    public class StatDiffProbe {
        private static void Out(string s) {
            File.AppendAllText("/tmp/stat_children.txt", s + "\n");
        }

        private static void Dump(Element el, int depth) {
            var pad = new string(' ', depth * 2);
            var line = pad + el.def.name + " [" + el.def.GetType().Name + "]";
            if (el is ValueElement ve) {
                var d = ve.data;
                if (d is byte[] b) line += " bytes[" + b.Length + "]";
                else if (d == null) line += " null";
                else line += " " + d.GetType().Name + "=" + d.ToString();
            }
            Out(line);
            if (el is Container c) {
                foreach (var kid in c.elements) Dump(kid, depth + 1);
            }
        }

        [Test]
        public void DumpStatDnamChildren() {
            File.Delete("/tmp/stat_children.txt");
            var session = new Session(Games.SSE, new SessionOptions());
            var path = TestHelpers.FixturePath("Skyrim.esm");
            if (!File.Exists(path)) Assert.Ignore("Skyrim.esm missing.");
            var plugin = session.pluginManager.LoadPlugin(path);
            var rec = plugin.GetRecordByFormId(0x1);
            Assert.NotNull(rec, "STAT 0x1 missing");
            Out("LOOKUP-SIG=" + rec.signature + " FORM=" + rec.formId.fileFormId.ToString("X8"));
            var statCount = 0;
            foreach (var r in plugin.records) {
                if (r.signature.ToString() == "STAT" && statCount < 3) {
                    Out("STAT-RECORD FORM=" + r.formId.fileFormId.ToString("X8"));
                    statCount++;
                }
                r._internalElements = null;
                r._unexpectedSubrecords = null;
            }
            var dnam = rec.GetElement("DNAM");
            Assert.NotNull(dnam, "DNAM missing");
            Out("DNAM-DEF-TYPE=" + dnam.def.GetType().FullName);
            Dump(dnam, 0);
            rec._internalElements = null;
            rec._unexpectedSubrecords = null;
        }
    }
}
