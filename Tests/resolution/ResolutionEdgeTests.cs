using esper;
using esper.data;
using esper.elements;
using esper.io;
using esper.plugins;
using esper.resolution;
using esper.setup;
using NUnit.Framework;
using System;
using System.Linq;

namespace Tests.resolution {
    // Covers the 8 wired resolution paths plus the public
    // extension-method edge cases (Ex-throwing variants, mutation
    // no-ops on missing paths, creation limits).
    public class ResolutionEdgeTests {
        private Session session;
        private PluginFile plugin;
        private MainRecord rec;

        [OneTimeSetUp]
        public void SetUp() {
            session = new Session(Games.SSE, new SessionOptions());
            plugin = session.pluginManager.LoadPlugin(
                TestHelpers.FixturePath("AllRecords.esp")
            );
            rec = (MainRecord)plugin.GetElement("ACTI\\[0]");
        }

        [Test]
        public void TestBySignaturePath() {
            var el = rec.GetElement("OBND");
            Assert.IsNotNull(el);
            Assert.AreEqual("OBND", el.def.signature.ToString());
        }

        [Test]
        public void TestByNamePath() {
            var obnd = rec.GetElement("OBND");
            Assert.IsNotNull(obnd);
            // resolving by display name finds the same element
            Assert.AreSame(obnd, rec.GetElement(obnd.name));
        }

        [Test]
        public void TestMissingPathReturnsNull() {
            Assert.IsNull(rec.GetElement("NOPE"));
            Assert.IsNull(rec.GetElement("OBND\\NOPE"));
        }

        [Test]
        public void TestGetElementExThrowsOnMissing() {
            var ex = Assert.Throws<Exception>(
                () => rec.GetElementEx("NOPE")
            );
            StringAssert.Contains("Failed to resolve element", ex.Message);
        }

        [Test]
        public void TestGetElementsExThrowsOnValue() {
            var ex = Assert.Throws<Exception>(
                () => rec.GetElementsEx("EDID")
            );
            StringAssert.Contains("does not have", ex.Message);
        }

        [Test]
        public void TestGetValueExThrowsOnMissing() {
            var ex = Assert.Throws<Exception>(
                () => rec.GetValueEx("NOPE")
            );
            StringAssert.Contains("Failed to resolve element", ex.Message);
        }

        [Test]
        public void TestGetDataExThrowsOnMissing() {
            var ex = Assert.Throws<Exception>(
                () => rec.GetDataEx<string>("NOPE")
            );
            StringAssert.Contains("Failed to resolve element", ex.Message);
        }

        [Test]
        public void TestByIndexPath() {
            var first = plugin.GetElement("ACTI\\[0]");
            Assert.IsNotNull(first);
            Assert.IsTrue(first is MainRecord);
        }

        [Test]
        public void TestByFileFormIdPath() {
            var byId = plugin.GetElement("&00000F55");
            Assert.AreSame(rec, byId);
        }

        [Test]
        public void TestParentNavigation() {
            var parent = rec.GetElement("OBND\\^Record");
            Assert.AreSame(rec, parent);
            var file = rec.GetElement("^File");
            Assert.AreSame(plugin, file);
            var group = rec.GetElement("^Group");
            Assert.IsTrue(group is GroupRecord);
        }

        [Test]
        public void TestContainerNavigation() {
            var el = rec.GetElement(@"OBND\..\EDID");
            Assert.IsNotNull(el);
            Assert.AreEqual(rec.GetValue("EDID"), el.GetValue());
        }

        [Test]
        public void TestReferencePath() {
            // the header FormID resolves to the record itself through
            // record injection (absent master materializes as a dummy)
            Assert.AreSame(rec, rec.GetElement(@"Record Header\@FormID"));
        }

        [Test]
        public void TestCreateElementUnsupportedThrows() {
            // '.' matches no child def, so creation throws rather
            // than returning null
            var ex = Assert.Throws<Exception>(
                () => rec.CreateElement(".")
            );
            StringAssert.Contains("Cannot create element", ex.Message);
        }

        [Test]
        public void TestCreateElementBySignatureReturnsExisting() {
            // creating a member that already exists returns it
            var obnd = rec.GetElement("OBND");
            Assert.AreSame(obnd, rec.CreateElement("OBND"));
        }

        [Test]
        public void TestCreateElementByNameOnRecordThrows() {
            // ResolveByName.Create -> CreateElementByName, which throws
            // for names outside the record's child defs
            var ex = Assert.Throws<Exception>(
                () => rec.CreateElement("Child Group")
            );
            StringAssert.Contains("Cannot create element", ex.Message);
        }

        [Test]
        public void TestSetValueRoundTrip() {
            var original = rec.GetValue("EDID");
            try {
                rec.SetValue("EDID", "ChangedName");
                Assert.AreEqual("ChangedName", rec.GetValue("EDID"));
                Assert.AreEqual("ChangedName", rec.editorId);
            } finally {
                rec.SetValue("EDID", original);
            }
            Assert.AreEqual(original, rec.GetValue("EDID"));
        }

        [Test]
        public void TestSetValueMissingIsSilentNoOp() {
            Assert.DoesNotThrow(() => rec.SetValue("NOPE", "x"));
            Assert.DoesNotThrow(() => rec.SetData("NOPE", 42));
            Assert.DoesNotThrow(() => rec.SetFlag("NOPE", "Ignored", true));
        }

        [Test]
        public void TestSetFlagRoundTrip() {
            var flags = rec.GetElement(@"Record Header\Record Flags");
            var original = flags.GetData<uint>();
            try {
                rec.SetFlag(@"Record Header\Record Flags", "Ignored", true);
                Assert.IsTrue(rec.GetFlag(
                    @"Record Header\Record Flags", "Ignored"
                ));
                rec.SetFlag(@"Record Header\Record Flags", "Ignored", false);
                Assert.IsFalse(rec.GetFlag(
                    @"Record Header\Record Flags", "Ignored"
                ));
            } finally {
                rec.SetData(@"Record Header\Record Flags", original);
            }
        }

        [Test]
        public void TestSetFlagUnknownThrows() {
            Assert.Throws<Exception>(
                () => rec.SetFlag(
                    @"Record Header\Record Flags", "Nope", true
                )
            );
        }

        [Test]
        public void TestGetFlagMissingIsFalse() {
            Assert.IsFalse(rec.GetFlag("NOPE", "Ignored"));
        }

        [Test]
        public void TestGetFlagExThrowsOnNonFlags() {
            var ex = Assert.Throws<Exception>(
                () => rec.GetFlagEx("EDID", "Ignored")
            );
            StringAssert.Contains("does not have flags", ex.Message);
        }

        [Test]
        public void TestAddElementMissingThrows() {
            var ex = Assert.Throws<Exception>(
                () => rec.AddElement("NOPE")
            );
            StringAssert.Contains("Cannot create element", ex.Message);
        }

        [Test]
        public void TestGetParentElement() {
            var obnd = rec.GetElement("OBND");
            var found = obnd.GetParentElement(e => e == rec);
            Assert.AreSame(rec, found);
            Assert.IsNull(obnd.GetParentElement(e => false));
        }

        [Test]
        public void TestGetRecordFlagOverloads() {
            // headerless dummy plugins null-guard to false
            var dummy = session.pluginManager.CreateDummyPlugin("dummy2.esp");
            try {
                var header = (MainRecord)dummy.GetElement("TES4");
                Assert.IsFalse(header.GetRecordFlag("Compressed"));
                Assert.IsFalse(header.GetRecordFlag(18));
            } finally {
                session.pluginManager.UnloadPlugin(dummy);
            }
        }
    }

    public class ElementStateTests {
        private Session session;
        private PluginFile plugin;
        private MainRecord rec;

        [OneTimeSetUp]
        public void SetUp() {
            session = new Session(Games.SSE, new SessionOptions());
            plugin = session.pluginManager.LoadPlugin(
                TestHelpers.FixturePath("AllRecords.esp")
            );
            rec = (MainRecord)plugin.GetElement("ACTI\\[0]");
        }

        [Test]
        public void TestSetAndClearState() {
            var el = rec.GetElement("EDID");
            Assert.AreEqual(ElementState.None, el.state);
            el.SetState(ElementState.Modified);
            Assert.AreEqual(ElementState.Modified, el.state);
            el.SetState(ElementState.Protected);
            Assert.AreEqual(
                ElementState.Modified | ElementState.Protected, el.state
            );
            // clearing a set flag removes it
            el.ClearState(ElementState.Modified);
            Assert.AreEqual(ElementState.Protected, el.state);
            // clearing an unset flag must NOT set it
            el.ClearState(ElementState.FilterShow);
            Assert.AreEqual(ElementState.Protected, el.state);
            el.ClearState(ElementState.Protected);
            Assert.AreEqual(ElementState.None, el.state);
        }

        [Test]
        public void TestRemoveRequiredReturnsFalse() {
            // OBND is required on ACTI (see SSE.json defs.ACTI.members)
            var obnd = rec.GetElement("OBND");
            Assert.IsTrue(obnd.def.required);
            Assert.IsFalse(obnd.Remove());
            Assert.IsNotNull(rec.GetElement("OBND"));
        }

        [Test]
        public void TestRemoveOptionalDetaches() {
            // PNAM (Marker Color) is optional on ACTI
            var pnam = rec.GetElement("PNAM");
            Assert.IsNotNull(pnam);
            Assert.IsTrue(pnam.Remove());
            Assert.IsNull(rec.GetElement("PNAM"));
        }

        [Test]
        public void TestSortKeyDelegatesToDef() {
            var el = rec.GetElement("EDID");
            Assert.AreEqual(el.def.GetSortKey(el), el.sortKey);
        }

        [Test]
        public void TestPathAndFullPath() {
            var el = rec.GetElement("EDID");
            Assert.AreEqual("EDID", el.path);
            StringAssert.Contains("EDID", el.fullPath);
        }

        [Test]
        public void TestSupportsSignature() {
            // the MainRecord override reports which member
            // signatures the record def can hold
            var obnd = Signature.FromString("OBND");
            var cell = Signature.FromString("CELL");
            Assert.IsTrue(rec.SupportsSignature(obnd));
            Assert.IsFalse(rec.SupportsSignature(cell));
        }

        [Test]
        public void TestMainRecordFormIds() {
            Assert.AreEqual(rec.fileFormId, rec.formId.fileFormId);
            Assert.AreEqual(
                rec.fileFormId & 0xFFFFFF, rec.localFormId
            );
            Assert.AreEqual(
                rec.fileFormId.ToString("X8"), rec.pathKey
            );
        }

        [Test]
        public void TestEditorIdSetter() {
            var original = rec.editorId;
            try {
                rec.editorId = "NewEditorId";
                Assert.AreEqual("NewEditorId", rec.GetValue("EDID"));
            } finally {
                rec.editorId = original;
            }
            Assert.AreEqual(original, rec.editorId);
        }

        [Test]
        public void TestValueElementDataRoundTrip() {
            var el = (ValueElement)rec.GetElement("EDID");
            var original = el.data;
            el.data = "TempName";
            Assert.AreEqual("TempName", el.value);
            el.data = original;
            Assert.AreEqual(original.ToDisplayString(), el.value);
        }

        [Test]
        public void TestRootElementInvariants() {
            var root = session.root;
            Assert.IsNull(root.sortKey);
            Assert.IsNull(root.path);
            Assert.IsNull(root.fullPath);
            Assert.AreEqual((UInt32)0, root.size);
            Assert.IsNull(root.file);
            Assert.IsNull(root.record);
            Assert.Throws<Exception>(() => root.Remove());
        }
    }
}
