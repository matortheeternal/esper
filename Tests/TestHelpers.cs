using esper.elements;
using esper.resolution;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace Tests {
    public static class TestHelpers {
        public static string fixturesPath = Path.Combine(
            Environment.CurrentDirectory, 
            "fixtures"
        );

        public static string FixturePath(string filename) {
            return Path.Combine(fixturesPath, filename);
        }

        public static void TestValue(Element element, string path, string expectedValue) {
            string value = element.GetValue(path);
            Assert.AreEqual(expectedValue, value);
        }

        public static void TestColor(Element element, string path) {
            var c = element.GetElement(path);
            Assert.AreEqual("255", c.GetValue("Red"));
            Assert.AreEqual("255", c.GetValue("Green"));
            Assert.AreEqual("255", c.GetValue("Blue"));
        }

        private static int GetDiffOffset(byte[] oldBytes, byte[] newBytes) {
            for (int i = 0; i < oldBytes.Length; i++)
                if (oldBytes[i] != newBytes[i]) return i;
            throw new Exception("No difference.");
        }

        public static void BinaryEqual(string oldPath, string newPath) {
            var oldStream = new FileStream(
                oldPath, FileMode.Open,
                FileAccess.Read, FileShare.ReadWrite
            );
            var oldReader = new BinaryReader(oldStream);
            var newStream = new FileStream(
                newPath, FileMode.Open,
                FileAccess.Read, FileShare.ReadWrite
            );
            var newReader = new BinaryReader(newStream);
            Assert.AreEqual(oldStream.Length, newStream.Length, "Length mismatch.");
            var i = 0;
            var len = oldStream.Length;
            while (i < len) {
                int bytesToRead = Math.Min(1024, (int) len - i);
                var oldBytes = oldReader.ReadBytes(bytesToRead);
                var newBytes = newReader.ReadBytes(bytesToRead);
                var equal = oldBytes.SequenceEqual(newBytes);
                var offset = equal ? 0 : i + GetDiffOffset(oldBytes, newBytes);
                Assert.IsTrue(equal, $"Bytes not equal at offset {offset}");
                i += bytesToRead;
            }
                
        }

        public static void TestFormId(Element element, string path) {
            Assert.AreEqual("{Null:000000}", element.GetValue(path));
        }

        public static void TestProp(
            Element script, 
            uint index, 
            string expectedPropName, 
            string expectedPropType, 
            Action<Element> testValue
        ) {
            var prop = script.GetElement(@$"Properties\[{index}]");
            Assert.IsNotNull(prop);
            Assert.AreEqual(expectedPropName, prop.GetValue("propertyName"));
            Assert.AreEqual(expectedPropType, prop.GetValue("Type"));
            Assert.AreEqual("Edited", prop.GetValue("Flags"));
            testValue(prop.GetElement("Value"));
        }

        public static void TestVMAD(Element element, Action<Element> testProps) {
            var vmad = element.GetElement("VMAD");
            Assert.IsNotNull(vmad);
            Assert.AreEqual("5", vmad.GetValue("Version"));
            Assert.AreEqual("2", vmad.GetValue("Object Format"));
            var script = vmad.GetElement(@"Scripts\[0]");
            Assert.IsNotNull(script);
            Assert.AreEqual("testScript", script.GetValue("ScriptName"));
            Assert.AreEqual("Local", script.GetValue("Flags")); // TODO?
            testProps(script);
        }

        public static void ExpectAllProps(Element script) {
            var props = script.GetElements(@"Properties");
            Assert.IsNotNull(props);
            Assert.AreEqual(11, props.Count);
            uint n = 0;
            TestProp(script, n++, "PropBool", "Bool", v => {
                Assert.AreEqual("True", v.GetValue());
            });
            TestProp(script, n++, "PropBoolArray", "Array of Bool", v => {
                var a = v.GetElement("[0]");
                Assert.AreEqual("True", a.GetValue("[0]"));
                Assert.AreEqual("False", a.GetValue("[1]"));
            });
            TestProp(script, n++, "PropFloat", "Float", v => {
                Assert.AreEqual("1.234000", v.GetValue());
            });
            TestProp(script, n++, "PropFloatArray", "Array of Float", v => {
                var a = v.GetElement("[0]");
                Assert.AreEqual("6.543000", a.GetValue("[0]"));
                Assert.AreEqual("-0.100000", a.GetValue("[1]"));
            });
            TestProp(script, n++, "PropInt32", "Int32", v => {
                Assert.AreEqual("3", v.GetValue());
            });
            TestProp(script, n++, "PropInt32Array", "Array of Int32", v => {
                var a = v.GetElement("[0]");
                Assert.AreEqual("123", a.GetValue("[0]"));
                Assert.AreEqual("-5", a.GetValue("[1]"));
            });
            TestProp(script, n++, "PropNone", "None", v => {
                Assert.AreEqual("", v.GetValue());
            });
            TestProp(script, n++, "PropObject", "Object", v => {
                var o = v.GetElement(@"[0]\[0]");
                TestFormId(o, "FormID");
                Assert.AreEqual("0", o.GetValue("Alias"));
            });
            TestProp(script, n++, "PropObjectArray", "Array of Object", v => {
                var o = v.GetElement(@"[0]\[0]\[0]");
                TestFormId(o, "FormID");
                Assert.AreEqual("0", o.GetValue("Alias"));
            });
            TestProp(script, n++, "PropString", "String", v => {
                Assert.AreEqual("Test", v.GetValue());
            });
            TestProp(script, n++, "PropStringArray", "Array of String", v => {
                var a = v.GetElement("[0]");
                Assert.AreEqual("Test", a.GetValue("[0]"));
                Assert.AreEqual("abcd", a.GetValue("[1]"));
            });
        }

        public static void TestEffects(Element rec) {
            var effects = rec.GetElements("Effects");
            Assert.IsNotNull(effects);
            Assert.AreEqual(1, effects.Count);
            TestFormId(effects[0], "EFID");
            var efit = effects[0].GetElement("EFIT");
            Assert.AreEqual("0.000000", efit.GetValue("Magnitude"));
            Assert.AreEqual("0", efit.GetValue("Area"));
            Assert.AreEqual("0", efit.GetValue("Duration"));
        }

        public static void TestObjectBounds(Element rec) {
            var obnd = rec.GetElement("OBND");
            Assert.IsNotNull(obnd);
            Assert.AreEqual("1", obnd.GetValue("X1"));
            Assert.AreEqual("2", obnd.GetValue("Y1"));
            Assert.AreEqual("3", obnd.GetValue("Z1"));
            Assert.AreEqual("4", obnd.GetValue("X2"));
            Assert.AreEqual("5", obnd.GetValue("Y2"));
            Assert.AreEqual("6", obnd.GetValue("Z2"));
        }

        public static void TestModel(
            Element rec, string path = "Model", bool testMODS = true
        ) {
            var model = rec.GetElement(path);
            Assert.IsNotNull(model);
            Assert.AreEqual(@"test\model\path", model.GetValue("[0]"));
            if (!testMODS) return;
            var n = (model as Container).elements.Count - 1;
            var alt = model.GetElement(@$"[{n}]\[0]");
            Assert.IsNotNull(alt);
            Assert.AreEqual("Test", alt.GetValue("3D Name"));
            TestFormId(alt, "New Texture");
            Assert.AreEqual("1", alt.GetValue("3D Index"));
        }

        public static void TestBodyTemplate(
            Element rec, 
            string fpFlags = "30 - Head, 31 - Hair",
            string armorType = "Light Armor"
        ) {
            var bt = rec.GetElement("Biped Body Template");
            Assert.IsNotNull(bt);
            Assert.AreEqual(fpFlags, bt.GetValue("First Person Flags"));
            Assert.AreEqual("", bt.GetValue("General Flags"));
            Assert.AreEqual(armorType, bt.GetValue("Armor Type"));
        }

        public static void TestIcon(Element rec) {
            var icon = rec.GetElement("Icon");
            Assert.IsNotNull(icon);
            Assert.AreEqual(@"large\icon\path", icon.GetValue("ICON"));
            Assert.AreEqual(@"small\icon\path", icon.GetValue("MICO"));
        }

        public static void TestDestructible(Element rec) {
            var destructible = rec.GetElement(@"Destructible\DEST");
            Assert.IsNotNull(destructible);
            Assert.AreEqual("100", destructible.GetValue("Health"));
            Assert.AreEqual("0", destructible.GetValue("DEST Count"));
            Assert.AreEqual("False", destructible.GetValue("VATS Targetable"));
            Assert.AreEqual("00 00", destructible.GetValue("Unknown"));
        }

        public static void TestKeywords(Element rec) {
            Assert.AreEqual("1", rec.GetValue("KSIZ"));
            var keywords = rec.GetElement("KWDA");
            Assert.IsNotNull(keywords);
            TestFormId(keywords, "[0]");
        }
    }
}
