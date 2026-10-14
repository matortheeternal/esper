using esper;
using esper.elements;
using esper.plugins;
using esper.setup;
using NUnit.Framework;
using System;
using System.IO;

namespace Tests.plugins {
    [TestFixture]
    public class InjectionTests {
        private string fixturePath;
        private Session session;
        private PluginFile fixture;
        private PluginFile dummy;

        private static void WriteSubrecord(BinaryWriter w, string sig, byte[] body) {
            w.Write(System.Text.Encoding.ASCII.GetBytes(sig));
            w.Write((UInt16)body.Length);
            w.Write(body);
        }

        private static byte[] CStr(string s) {
            return System.Text.Encoding.ASCII.GetBytes(s + "\0");
        }

        [OneTimeSetUp]
        public void BuildFixtureAndLoad() {
            var dir = Path.Combine(Path.GetTempPath(), "esper-injection");
            Directory.CreateDirectory(dir);
            fixturePath = Path.Combine(dir, "Inject.fixture.esp");
            using (var stream = new FileStream(fixturePath, FileMode.Create)) {
                var w = new BinaryWriter(stream);
                var rec1 = new MemoryStream();
                var rw = new BinaryWriter(rec1);
                rw.Write(System.Text.Encoding.ASCII.GetBytes("ACTI"));
                var edid = System.Text.Encoding.ASCII.GetBytes("TestACTI\0");
                rw.Write((UInt32)(4 + 2 + edid.Length));
                rw.Write((UInt32)0);
                rw.Write((UInt32)0x1B);
                rw.Write(new byte[8]);
                WriteSubrecord(rw, "EDID", edid);
                var rec2 = new MemoryStream();
                rw = new BinaryWriter(rec2);
                rw.Write(System.Text.Encoding.ASCII.GetBytes("ACTI"));
                rw.Write((UInt32)0);
                rw.Write((UInt32)0);
                rw.Write((UInt32)0x7F);
                rw.Write(new byte[8]);
                var b1 = rec1.ToArray();
                var b2 = rec2.ToArray();
                w.Write(System.Text.Encoding.ASCII.GetBytes("TES4"));
                var header = new MemoryStream();
                var hw = new BinaryWriter(header);
                hw.Write(1.7f);
                hw.Write((UInt32)2);
                hw.Write((UInt32)0x800);
                var hbytes = header.ToArray();
                var hsub = new MemoryStream();
                var sw = new BinaryWriter(hsub);
                WriteSubrecord(sw, "HEDR", hbytes);
                WriteSubrecord(sw, "CNAM", CStr("InjectionTest"));
                WriteSubrecord(sw, "MAST", CStr("Skyrim.esm"));
                WriteSubrecord(sw, "DATA", new byte[8]);
                var hsubBytes = hsub.ToArray();
                w.Write((UInt32)hsubBytes.Length);
                w.Write((UInt32)0);
                w.Write((UInt32)0);
                w.Write(new byte[8]);
                w.Write(hsubBytes);
                w.Write(System.Text.Encoding.ASCII.GetBytes("GRUP"));
                w.Write((UInt32)(24 + b1.Length + b2.Length));
                w.Write(System.Text.Encoding.ASCII.GetBytes("ACTI"));
                w.Write((Int32)0);
                w.Write((UInt32)0);
                w.Write((UInt32)0);
                w.Write(b1);
                w.Write(b2);
            }
            session = new Session(Games.SSE, new SessionOptions());
            fixture = session.pluginManager.LoadPlugin(fixturePath);
            dummy = session.pluginManager.GetFileByName("Skyrim.esm");
        }

        [OneTimeTearDown]
        public void Cleanup() {
            try { File.Delete(fixturePath); } catch { }
        }

        [Test]
        public void AbsentMasterIsAutoDummied() {
            Assert.IsNotNull(dummy);
            Assert.IsTrue(dummy.isDummy);
        }

        [Test]
        public void ExactLookupOnOwningFileIsUnaffected() {
            var rec = fixture.GetRecordByFormId(0x1B);
            Assert.IsNotNull(rec);
            Assert.AreEqual("ACTI", rec.signature.ToString());
        }

        [Test]
        public void DummyMissFallsBackToInjectedRecord() {
            var rec = dummy.GetRecordByFormId(0x1B);
            Assert.IsNotNull(rec);
            Assert.AreSame(fixture.GetRecordByFormId(0x1B), rec);
        }

        [Test]
        public void InjectedRecordReportsIsInjected() {
            var rec = fixture.GetRecordByFormId(0x1B);
            Assert.IsTrue(rec.isInjected);
        }

        [Test]
        public void RecordsBelowHardcodedRangeStillInject() {
            var rec = fixture.GetRecordByFormId(0x7F);
            Assert.IsNotNull(rec);
            Assert.IsTrue(rec.isInjected);
            Assert.AreSame(rec, dummy.GetRecordByFormId(0x7F));
        }

        [Test]
        public void MissReturnsNullWithoutThrowing() {
            Assert.IsNull(dummy.GetRecordByFormId(0xFFFFFF));
            Assert.IsNull(fixture.GetRecordByFormId(0xFFFFFF));
        }

        [Test]
        public void InjectionsCanBeDisabled() {
            var s2 = new Session(Games.SSE, new SessionOptions {
                allowInjections = false
            });
            var f2 = s2.pluginManager.LoadPlugin(fixturePath);
            var d2 = s2.pluginManager.GetFileByName("Skyrim.esm");
            Assert.IsNotNull(d2);
            Assert.IsNull(d2.GetRecordByFormId(0x1B));
            Assert.IsNotNull(f2.GetRecordByFormId(0x1B));
        }

        [Test]
        public void InjectedRecordOverridesDoNotRecurse() {
            var rec = dummy.GetRecordByFormId(0x1B);
            Assert.IsNotNull(rec.overrides);
            Assert.IsNotNull(rec.referencedBy);
        }
    }
}
