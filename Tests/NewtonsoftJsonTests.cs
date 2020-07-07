using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Diagnostics;
using System.IO;
using esper;

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

            Assert.IsTrue(definitions.ContainsKey("ARMO"));
            TestContext.Out.WriteLine("Loaded definitions in {0} ms", stopwatch.ElapsedMilliseconds);
            Assert.Pass();
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
            Assert.Pass();
        }
    }
}