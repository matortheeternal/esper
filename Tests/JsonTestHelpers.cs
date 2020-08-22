using esper.defs;
using esper.elements;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Tests {
    public static class JsonTestHelpers {
        private static void CheckKey(JObject json, string key, Element parent) {
            Assert.IsTrue(json.ContainsKey(key), 
                $"Element {key} should not be in {parent.path}"
            );
        }

        private static void ExpectAllPropertiesFound(JObject json, Element element) {
            Assert.AreEqual(0, json.Count,
                $"The following properties were not found in {element.path}: " +
                $"{json.Properties().Select(p => p.Name)}"
            );
        }

        private static void TestFloatValue(
            ValueElement element, float expectedFloat
        ) {
            Assert.IsNotNull(element.data);
            float actualFloat = element.data;
            float diff = expectedFloat - actualFloat;
            Assert.IsTrue(diff <= element.sessionOptions.epsilon,
                $"Float values were not close {element.path}, diff: {diff}"
            );
        }

        private static void TestElementValue(Element element, string value) {
            if (element is StructElement s && value == "") {
                Assert.AreEqual(0, s.count, "Struct should have 0 children.");
                return;
            }
            var v = (ValueElement)element;
            Assert.IsNotNull(v,
                $"Expected {element.path} to have value {value}, "+
                "but it's not a value element."
            );
            if (v.def is FloatDef floatDef && floatDef.formatDef == null) {
                if (float.TryParse(value, out float fValue)) {
                    TestFloatValue(v, fValue);
                    return;
                }
                if (value == "") {
                    Assert.IsNull(v.data);
                    return;
                }
            }
            Assert.AreEqual(value, v.value,
                $"Values did not match {element.fullPath}"
            );
        }

        private static void TestElement(Element element, JObject json) {
            if (json.ContainsKey("value")) {
                TestElementValue(element, json.Value<string>("value"));
            } else if (json.ContainsKey("elements")) {
                TestContainerValues(element, json.Value<JArray>("elements"));
            } else {
                throw new Exception($"Error in JSON, {json}");
            }
        }

        private static void TestContainerValues(Element element, JArray array) {
            var c = (Container)element;
            Assert.IsNotNull(c, $"Expected {element.displayName} to be a container");
            Assert.AreEqual(array.Count, c.count, 
                $"Element count mismatch, {c.displayName}"
            );
            for (int i = 0; i < c.count; i++) {
                var e = c.elements[i];
                TestElement(e, array.Value<JObject>(i));
            }
        }

        private static void TestElementBuildTime(MainRecord rec) {
            var sw = Stopwatch.StartNew();
            Assert.IsNotNull(rec.count);
            sw.Stop();
            var elapsedTime = sw.ElapsedTicks / (double) Stopwatch.Frequency * 1000;
            Console.WriteLine($"Built elements for {rec.signature} in {elapsedTime:N3}ms");
        }

        private static void TestRecordValues(JObject json, MainRecord rec) {
            TestElementBuildTime(rec);
            TestContainerValues(rec, json.Value<JArray>("elements"));
            if (json.ContainsKey("Child Group")) {
                var src = json.Value<JObject>("Child Group");
                TestGroupValues(src, rec.childGroup);
            }
        }

        private static void TestGroupValues(JObject json, GroupRecord group) {
            group.elements.ForEach(element => {
                if (element is MainRecord rec) {
                    var key = rec.formId.ToString("X8");
                    CheckKey(json, key, group);
                    TestRecordValues(json.Value<JObject>(key), rec);
                    json.Remove(key);
                } else if (element is GroupRecord innerGroup) {
                    if (innerGroup.isChildGroup) return;
                    var key = innerGroup.name;
                    CheckKey(json, key, group);
                    TestGroupValues(json.Value<JObject>(key), innerGroup);
                    json.Remove(key);
                }
            });
            ExpectAllPropertiesFound(json, group);
        }

        public static void TestJsonValues(string name, Element element) {
            var filePath = TestHelpers.FixturePath($@"AllRecords\{name}.json");
            if (!File.Exists(filePath)) {
                Console.WriteLine($"Fixture not found, skipping: {name}");
                return;
            }
            var json = JObject.Parse(File.ReadAllText(filePath));
            if (element is MainRecord rec) {
                TestRecordValues(json, rec);
            } else if (element is GroupRecord group) {
                TestGroupValues(json, group);
            }
        }
    }
}
