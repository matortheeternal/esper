using esper.elements;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace Tests {
    public static class JsonTestHelpers {
        private static void CheckKey(JObject json, string key, Element parent) {
            Assert.IsTrue(json.ContainsKey(key), 
                $"Element {key} not found in {parent.displayName}"
            );
        }

        private static void ExpectAllPropertiesFound(JObject json, Element element) {
            Assert.AreEqual(0, json.Count,
                $"The following properties were not found in {element.displayName}:" +
                $"{json.Properties().Select(p => p.Name)}"
            );
        }

        private static void TestElementValue(Element element, string value) {
            var v = (ValueElement)element;
            Assert.IsNotNull(v,
                $"Expected {element.displayName} to have value {value}, but it's not a value element."
            );
            Assert.AreEqual(value, v.value, 
                $"Values did not match {element.displayName}"
            );
        }

        private static void TestObjectValues(Element element, JObject obj) {
            var c = (Container)element;
            Assert.IsNotNull(c, $"Expected {element.displayName} to be a container");
            c.elements.ForEach(e => TestElement(obj, e, c));
            ExpectAllPropertiesFound(obj, c);
        }

        private static void TestArrayValues(Element element, JArray array) {
            var c = (Container)element;
            Assert.IsNotNull(c, $"Expected {element.displayName} to be a container");
            Assert.AreEqual(array.Count, c.count, $"Element count mismatch, {c.displayName}");
            for (int i = 0; i < c.count; i++) {
                var e = c.elements[i];
                var type = array[i].Type;
                if (type == JTokenType.String) {
                    TestElementValue(e, array.Value<string>(i));
                } else if (type == JTokenType.Object) {
                    TestObjectValues(e, array.Value<JObject>(i));
                } else if (type == JTokenType.Array) {
                    TestArrayValues(e, array.Value<JArray>(i));
                } else {
                    throw new Exception($"Error in JSON, {i}");
                }
            }
        }

        private static void TestElement(JObject json, Element element, Container container) {
            var key = element.displayName;
            CheckKey(json, key, container);
            var type = json[key].Type;
            if (type == JTokenType.String) {
                TestElementValue(element, json.Value<string>(key));
            } else if (type == JTokenType.Object) {
                TestObjectValues(element, json.Value<JObject>(key));
            } else if (type == JTokenType.Array) {
                TestArrayValues(element, json.Value<JArray>(key));
            } else {
                throw new Exception($"Error in JSON, {key}");
            }
            json.Remove(key);
        }

        private static void TestRecordValues(JObject json, MainRecord rec) {
            rec.elements.ForEach(e => TestElement(json, e, rec));
            ExpectAllPropertiesFound(json, rec);
        }

        private static void TestGroupValues(JObject json, GroupRecord group) {
            group.elements.ForEach(element => {
                if (element is MainRecord rec) {
                    var key = rec.formId.ToString("X8");
                    CheckKey(json, key, group);
                    TestRecordValues(json.Value<JObject>(key), rec);
                    json.Remove(key);
                } else if (element is GroupRecord innerGroup) {
                    var key = group.name;
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
