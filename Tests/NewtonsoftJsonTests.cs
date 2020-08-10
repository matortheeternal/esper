using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Diagnostics;
using System.IO;
using esper.helpers;
using System.Collections.Generic;

namespace Tests {
    public class NewtonsoftJsonTests {
        string definitionsPath;
        Stopwatch stopwatch;
        JObject definitions;
        readonly JObject emptyObj = new JObject {};

        [OneTimeSetUp]
        public void Setup() {
            definitionsPath = @"C:\dev\git\esp.json\data\TES5.json";
            stopwatch = new Stopwatch();
        }

        [TearDown]
        public void Cleanup() {
            stopwatch.Reset();
        }

        [Test]
        public void TestParsing() {
            stopwatch.Start();
            string json = File.ReadAllText(definitionsPath);
            definitions = JObject.Parse(json);
            stopwatch.Stop();

            Assert.IsTrue(definitions.Value<JObject>("defs").ContainsKey("ARMO"));
            TestContext.Out.WriteLine("Loaded definitions in {0} ms", stopwatch.ElapsedMilliseconds);
        }

        [Test]
        public void TestMerging() {
            stopwatch.Start();
            JObject obj = JsonHelpers.ObjectAssign(
                new JObject(emptyObj),
                JObject.Parse("{ a: 1, b: 2 }"),
                JObject.Parse("{ c: 3, d: 4 }"),
                JObject.Parse("{ c: 5, d: 6 }")
            );
            stopwatch.Stop();
            Assert.AreEqual(obj.Value<int>("a"), 1);
            Assert.AreEqual(obj.Value<int>("b"), 2);
            Assert.AreEqual(obj.Value<int>("c"), 5);
            Assert.AreEqual(obj.Value<int>("d"), 6);
        }

        [Test]
        public void TestMissingProperties() {
            var obj = JObject.Parse("{ a: 1, b: 2 }");
            Assert.IsNull(obj.Value<int?>("test"));
            Assert.IsNull(obj.Value<string>("test"));
            Assert.IsFalse(obj.Value<bool>("test"));
        }

        [Test]
        public void TestListValues() {
            var obj = JObject.Parse("{ a: [0], b: [\"a\", \"b\", \"c\"] }");
            var numbers = obj.Value<JToken>("a").ToObject<List<int>>();
            Assert.IsNotNull(numbers);
            Assert.AreEqual(1, numbers.Count);
            Assert.AreEqual(0, numbers[0]);
            var strings = obj.Value<JToken>("b").ToObject<List<string>>();
            Assert.IsNotNull(strings);
            Assert.AreEqual(3, strings.Count);
            Assert.AreEqual("a", strings[0]);
            Assert.AreEqual("b", strings[1]);
            Assert.AreEqual("c", strings[2]);
        }
    }
}