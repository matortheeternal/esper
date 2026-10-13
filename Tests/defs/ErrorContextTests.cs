using System;
using System.IO;
using esper;
using esper.elements;
using esper.io;
using esper.plugins;
using esper.resolution;
using esper.setup;
using NUnit.Framework;

namespace Tests.defs {
    [TestFixture]
    public class ErrorContextTests {
        private static PluginFile LoadAllRecords(Session session) {
            var path = TestHelpers.FixturePath("AllRecords.esp");
            if (!File.Exists(path))
                Assert.Ignore("AllRecords.esp missing.");
            return session.pluginManager.LoadPlugin(path);
        }

        private static ValueElement FindStringElement() {
            var session = new Session(Games.SSE, new SessionOptions());
            var plugin = LoadAllRecords(session);
            foreach (var record in plugin.records) {
                if (record.GetElement("EDID") is ValueElement edid)
                    return edid;
            }
            Assert.Ignore("No EDID element in AllRecords.esp.");
            return null;
        }

        [Test]
        public void ReadMultipleReportsPositionAndBounds() {
            var source = new RecordSource(null, new byte[16]);
            var ex = Assert.Throws<Exception>(() =>
                source.ReadMultiple(
                    4, () => { source.reader.ReadBytes(8); }
                )
            );
            Assert.That(ex.Message, Does.Contain("Position: 0x"));
            Assert.That(ex.Message, Does.Contain("end offset: 0x"));
            Assert.That(ex.Message, Does.Contain("data size: 4"));
        }

        [Test]
        public void ElementsGetterNamesTheElement() {
            var edid = FindStringElement();
            var ex = Assert.Throws<Exception>(() => {
                var _ = edid.elements;
            });
            Assert.That(ex.Message, Does.Contain("'Editor ID'"));
            Assert.That(ex.Message, Does.Contain("not a container"));
        }

        [Test]
        public void InternalElementsGetterNamesTheElement() {
            var edid = FindStringElement();
            var ex = Assert.Throws<Exception>(() => {
                var _ = edid.internalElements;
            });
            Assert.That(ex.Message, Does.Contain("'Editor ID'"));
            Assert.That(ex.Message, Does.Contain("not a container"));
        }

        [Test]
        public void ReferencedRecordGetterNamesTheElement() {
            var edid = FindStringElement();
            var ex = Assert.Throws<Exception>(() => {
                var _ = edid.referencedRecord;
            });
            Assert.That(ex.Message, Does.Contain("'Editor ID'"));
            Assert.That(ex.Message, Does.Contain("not reference records"));
        }

        [Test]
        public void ReferencedRecordSetterNamesTheElement() {
            var header = FindRecordHeader();
            var ex = Assert.Throws<Exception>(() => {
                header.referencedRecord = null;
            });
            Assert.That(ex.Message, Does.Contain("'Record Header'"));
        }

        private static StructElement FindRecordHeader() {
            var path = TestHelpers.FixturePath("AllRecords.esp");
            if (!File.Exists(path))
                Assert.Ignore("AllRecords.esp missing.");
            var session = new Session(Games.SSE, new SessionOptions());
            var plugin = session.pluginManager.LoadPlugin(path);
            foreach (var record in plugin.records)
                if (record.GetElement("Record Header") is StructElement header)
                    return header;
            Assert.Fail("No Record Header found.");
            return null;
        }
    }
}
