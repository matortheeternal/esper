using esper;
using esper.defs;
using esper.elements;
using esper.io;
using esper.plugins;
using esper.resolution;
using esper.setup;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Tests;

namespace Tests.defs {
    // Proves xEdit's recursive defs are transparent array references:
    // both the union arms (levelsUp 3) and the array element (levelsUp 4)
    // of FO4's ScriptPropertyStruct resolve to the 'Struct' array def,
    // so nested structs decode as plain ArrayElements and round-trip.
    [TestFixture]
    public class RecursiveDefTests {
        // Nested struct: top Struct[Member "Nested"/7] -> nested
        // Struct[Member "Items"/15] -> Array of Struct[Struct[Member
        // "Answer"/3] = 42]. Type codes follow ScriptPropertyDecider:
        // <=7 selects the arm directly, 11..15 selects array arms
        // (16 = Array of Struct), 3 = Int32.
        private static readonly byte[] nestedStructBytes = {
            0x01, 0x00, 0x00, 0x00,             // top count = 1
            0x06, 0x00,                         // len("Nested")
            0x4E, 0x65, 0x73, 0x74, 0x65, 0x64, // "Nested"
            0x07,                               // Type = Struct
            0x00,                               // Flags
            0x01, 0x00, 0x00, 0x00,             // nested count = 1
            0x05, 0x00,                         // len("Items")
            0x49, 0x74, 0x65, 0x6D, 0x73,       // "Items"
            0x11,                               // Type = Array of Struct
            0x00,                               // Flags
            0x01, 0x00, 0x00, 0x00,             // array count = 1
            0x01, 0x00, 0x00, 0x00,             // resolved count = 1
            0x06, 0x00,                         // len("Answer")
            0x41, 0x6E, 0x73, 0x77, 0x65, 0x72, // "Answer"
            0x03,                               // Type = Int32
            0x00,                               // Flags
            0x2A, 0x00, 0x00, 0x00              // 42
        };

        private static ArrayDef StructArrayDef(DefinitionManager manager) {
            // NOTE: BuildDef({id}) cannot be used here: the manager drops
            // the raw JSON (definitions.Remove("defs")) after eagerly
            // building every def, so id lookup must go via the def cache.
            return (ArrayDef)manager.ResolveDef("ScriptPropertyStruct");
        }

        [Test]
        public void ScriptPropertyStructRecursiveArms() {
            var manager = Synthetic.GetSession(Games.FO4).definitionManager;
            var arrayDef = StructArrayDef(manager);
            Assert.AreEqual("Struct", arrayDef.name);
            var member = (StructDef)arrayDef.elementDef;
            var value = (UnionDef)member.elementDefs
                .First(def => def.name == "Value");
            var arms = value.elementDefs;
            Assert.AreEqual(3, ((RecursiveDef)arms[6]).levelsUp);
            Assert.AreEqual(3, ((RecursiveDef)arms[7]).levelsUp);
            var arrayOfStruct = (ArrayDef)arms[13];
            Assert.AreEqual("Array of Struct", arrayOfStruct.name);
            Assert.AreEqual(
                4, ((RecursiveDef)arrayOfStruct.elementDef).levelsUp);
        }

        [Test]
        public void RecursiveDefRejectsNonPositiveLevelsUp() {
            var manager = Synthetic.GetSession(Games.FO4).definitionManager;
            var src = JObject.Parse(
                "{'type':'recursive','name':'Bad','levelsUp':0}");
            var ex = Assert.Throws<TargetInvocationException>(
                () => manager.BuildDef(src));
            StringAssert.Contains("levelsUp", ex.InnerException.Message);
        }

        // Struct reads stop at source.dataEndPos, which RecordSource
        // only sets from subrecord framing. Raw synthetic bytes have no
        // framing, so expose the whole buffer as readable.
        private class TestSource : RecordSource {
            private readonly PluginFile _plugin;
            internal TestSource(byte[] data, PluginFile plugin)
                : base(null, data) {
                _plugin = plugin;
            }
            internal override long dataEndPos => stream.Length;
            internal override PluginFile plugin => _plugin;
        }

        [Test]
        public void NestedStructsDecodeAsPlainArrays() {
            var manager = Synthetic.GetSession(Games.FO4).definitionManager;
            var arrayDef = StructArrayDef(manager);
            var plugin = Fixture.Master(Games.FO4, "Fallout4.esm");
            var source = new TestSource(nestedStructBytes, plugin);
            var top = (ArrayElement)arrayDef.ReadElement(null, source);
            Assert.AreEqual("Struct", top.name);
            Assert.AreEqual(1, top.elements.Count);

            var nestedMember = (StructElement)top.elements[0];
            Assert.AreEqual("Member", nestedMember.name);
            Assert.AreEqual(
                "Nested", nestedMember.GetData<string>("memberName"));
            Assert.AreEqual(7, nestedMember.GetData<byte>("Type"));
            var valueEl = nestedMember.GetElement("Value");
            var nestedUnion = valueEl;
            // Union arms read into a UnionElement wrapper; the recursive
            // arm itself resolves to a plain array element inside it.
            Assert.AreEqual(typeof(UnionElement), nestedUnion.GetType());
            var nested = (ArrayElement)nestedUnion.elements[0];
            Assert.AreEqual("Struct", nested.name);

            var itemsMember = (StructElement)nested.elements[0];
            Assert.AreEqual(
                "Items", itemsMember.GetData<string>("memberName"));
            Assert.AreEqual(17, itemsMember.GetData<byte>("Type"));
            var arrayOfStructUnion = itemsMember.GetElement("Value");
            Assert.AreEqual(typeof(UnionElement), arrayOfStructUnion.GetType());
            var arrayOfStruct = (ArrayElement)arrayOfStructUnion.elements[0];
            Assert.AreEqual("Array of Struct", arrayOfStruct.name);
            Assert.AreEqual(1, arrayOfStruct.elements.Count);

            var resolved = (ArrayElement)arrayOfStruct.elements[0];
            Assert.AreEqual(typeof(ArrayElement), resolved.GetType());
            Assert.AreEqual("Struct", resolved.name);
            Assert.AreEqual(1, resolved.elements.Count);

            var answerMember = (StructElement)resolved.elements[0];
            Assert.AreEqual(
                "Answer", answerMember.GetData<string>("memberName"));
            Assert.AreEqual(3, answerMember.GetData<byte>("Type"));
            Assert.AreEqual(42, answerMember.GetData<int>("Value"));
        }

        [Test]
        public void NestedStructsWriteByteIdentical() {
            var session = new Session(Games.FO4, new SessionOptions());
            var plugin =
                Fixture.LoadFreshInto(session, Games.FO4, "Fallout4.esm");
            var arrayDef = StructArrayDef(session.definitionManager);
            var source = new TestSource(nestedStructBytes, plugin);
            var top = arrayDef.ReadElement(null, source);
            var path = Path.GetTempFileName();
            try {
                var output = new PluginFileOutput(path, plugin);
                arrayDef.WriteElement(top, output);
                output.writer.Flush();
                output.writer.Close();
                CollectionAssert.AreEqual(
                    nestedStructBytes, File.ReadAllBytes(path));
            } finally {
                File.Delete(path);
            }
        }
    }
}
