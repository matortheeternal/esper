using esper;
using esper.data;
using esper.defs;
using esper.defs.FO4;
using esper.elements;
using esper.plugins;
using esper.resolution;
using esper.setup;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Tests;
using balsa.stringtables;

namespace Tests.defs {
    public class Batch3FormatTests {
        private static DefinitionManager Fo4Manager() {
            return Synthetic.GetSession(Games.FO4).definitionManager;
        }

        private static PluginFile LoadOrIgnore(Session session, string name) {
            var path = TestHelpers.FixturePath(name);
            if (!File.Exists(path)) Assert.Ignore(name + " missing.");
            return session.pluginManager.LoadPlugin(path);
        }

        private static Session NewSession() {
            return new Session(Games.FO4, new SessionOptions());
        }

        private static void LoadSyntheticStrings(
            PluginFile plugin, Dictionary<uint, string> entries) {
            var stringFile = new TES5StringFile("synthetic_english.strings");
            typeof(StringFile)
                .GetField(
                    "_strings",
                    BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(
                    stringFile, new Dictionary<uint, string>(entries));
            plugin.stringFiles =
                new Dictionary<StringFileType, StringFile> {
                    { StringFileType.STRINGS, stringFile },
                };
        }

        private static ValueElement Walk(Element el, string defId) {
            if (el is ValueElement ve && ve.def is ValueDef vd &&
                vd.formatDef != null && vd.formatDef.GetType().Name == defId)
                return ve;
            if (el is Container c && c.elements != null) {
                foreach (var child in c.elements) {
                    var hit = Walk(child, defId);
                    if (hit != null) return hit;
                }
            }
            return null;
        }

        [Test]
        public void IntToHexStrDisplay() {
            var format = new IntToHexStrFormat(
                Fo4Manager(), new JObject());
            Assert.AreEqual("00000005", format.DataToValue(null, 5u));
            Assert.AreEqual("00000000", format.DataToValue(null, 0u));
            Assert.AreEqual(
                "FF804020", format.DataToValue(null, 0xFF804020u));
        }

        [Test]
        public void WbHexStrToIntArms() {
            Assert.AreEqual(5u, Fo4FormatHelpers.WbHexStrToInt("00000005"));
            Assert.AreEqual(
                5u, Fo4FormatHelpers.WbHexStrToInt("00000005 Extra Name"));
            Assert.AreEqual(
                5u, Fo4FormatHelpers.WbHexStrToInt("00000005:Extra"));
            Assert.AreEqual(0u, Fo4FormatHelpers.WbHexStrToInt("$5"));
            Assert.AreEqual(0u, Fo4FormatHelpers.WbHexStrToInt("ZZZ"));
            Assert.AreEqual(0u, Fo4FormatHelpers.WbHexStrToInt(""));
            Assert.AreEqual(0u, Fo4FormatHelpers.WbHexStrToInt(null));
            var format = new IntToHexStrFormat(
                Fo4Manager(), new JObject());
            Assert.AreEqual(5u, format.ValueToData(null, "00000005 Name"));
        }

        [Test]
        public void CLFMRgbaDisplayAndSortKey() {
            var format = new CLFMColorFormat(
                Fo4Manager(), new JObject());
            Assert.AreEqual(
                "rgba(32, 64, 128, 255)",
                format.DataToValue(null, 0xFF804020u));
            Assert.AreEqual(
                "FF804020", format.GetSortKey(null, 0xFF804020u));
            Assert.AreEqual(
                "rgba(0, 0, 0, 0)", format.DataToValue(null, 0u));
        }

        [Test]
        public void CLFMValueToData() {
            var format = new CLFMColorFormat(
                Fo4Manager(), new JObject());
            Assert.AreEqual(
                0xFF804020u, format.ValueToData(null, "rgba(32, 64, 128, 255)"));
            Assert.AreEqual(
                0xFF804020u, format.ValueToData(null, "RGBA(32,64,128,255)"));
            Assert.AreEqual(0u, format.ValueToData(null, "rgba(1, 2, 3)"));
            Assert.AreEqual(0u, format.ValueToData(null, "rgba(1, 2, 3"));
            Assert.AreEqual(
                BitConverter.ToUInt32(BitConverter.GetBytes(1.5f), 0),
                format.ValueToData(null, "1.5"));
            Assert.AreEqual(0u, format.ValueToData(null, "junk"));
        }

        [Test]
        public void CLFMFloatArm() {
            var Mage = Synthetic.NewStruct(Games.FO4,
                "{\"name\":\"ColorStruct\",\"type\":\"struct\",\"elements\":[" +
                "{\"name\":\"Color\",\"type\":\"uint32\"," +
                "\"format\":{\"type\":\"CLFMColorFormat\"}}," +
                "{\"name\":\"FNAM\",\"type\":\"uint32\"}]}");
            var color = Mage.GetElement("Color") as ValueElement;
            var fnam = Mage.GetElement("FNAM") as ValueElement;
            Assert.NotNull(color);
            Assert.NotNull(fnam);
            fnam.data = 2u;
            var bits = BitConverter.ToUInt32(BitConverter.GetBytes(2.5f), 0);
            Assert.AreEqual("2.500000", color.def is ValueDef vd
                ? vd.formatDef.DataToValue(color, bits)
                : null);
        }

        [Test]
        public void FaceMorphBareIndex() {
            var format = new FaceMorphFormat(
                Fo4Manager(), new JObject());
            Assert.AreEqual("00000007", format.DataToValue(null, 7u));
            Assert.AreEqual(
                "00000007", format.GetSortKey(null, 7u));
            Assert.AreEqual(7u, format.ValueToData(null, "00000007 Name"));
        }

        [Test]
        public void MorphValueBareIndex() {
            var format = new MorphValueFormat(
                Fo4Manager(), new JObject());
            Assert.AreEqual("00000009", format.DataToValue(null, 9u));
            Assert.AreEqual(
                "00000009", format.GetSortKey(null, 9u));
            Assert.AreEqual(9u, format.ValueToData(null, "00000009:Name"));
        }

        [Test]
        public void WiringCLFMColor() {
            var session = NewSession();
            var plugin = LoadOrIgnore(session, "Fallout4.esm");
            var rec = plugin.records.FirstOrDefault(r =>
                r.signature.ToString() == "CLFM");
            Assert.NotNull(rec, "No CLFM record decoded.");
            var color = Walk(rec, "CLFMColorFormat");
            Assert.NotNull(color, "No CLFMColorFormat element decoded.");
            var text = ((ValueDef)color.def).formatDef.DataToValue(
                color, color.data);
            var flags = color.GetData<uint?>(@"..\FNAM") ?? 0;
            if ((flags & 2) != 0)
                Assert.That(text, Does.Match(@"^-?\d+\.\d{6}$"));
            else
                Assert.That(text, Does.StartWith("rgba("));
        }

        [Test]
        public void WiringIntToHexStr() {
            var session = NewSession();
            var plugin = LoadOrIgnore(session, "Fallout4.esm");
            var rec = plugin.records.FirstOrDefault(r =>
                r.signature.ToString() == "RACE");
            Assert.NotNull(rec, "No RACE record decoded.");
            var index = Walk(rec, "IntToHexStrFormat");
            Assert.NotNull(index, "No IntToHexStrFormat element decoded.");
            Assert.AreEqual(
                Convert.ToUInt32(index.data).ToString("X8"),
                ((ValueDef)index.def).formatDef.DataToValue(
                    index, index.data));
        }

        [Test]
        public void WiringFaceMorph() {
            var session = NewSession();
            var plugin = LoadOrIgnore(session, "Fallout4.esm");
            var found = false;
            foreach (var rec in plugin.records
                .Where(r => r.signature.ToString() == "NPC_")
                .Take(500)) {
                var morph = Walk(rec, "FaceMorphFormat");
                if (morph == null) continue;
                found = true;
                var text = ((ValueDef)morph.def).formatDef.DataToValue(
                    morph, morph.data);
                Assert.That(
                    text,
                    Does.StartWith(
                        Convert.ToUInt32(morph.data).ToString("X8")));
                break;
            }
            Assert.IsTrue(found, "No FaceMorphFormat element decoded.");
        }

        [Test]
        public void FaceMorphResolvedName() {
            var session = NewSession();
            var plugin = LoadOrIgnore(session, "Fallout4.esm");
            foreach (var rec in plugin.records
                .Where(r => r.signature.ToString() == "NPC_")
                .Take(500)) {
                var morph = Walk(rec, "FaceMorphFormat");
                if (morph == null) continue;
                var race = (MainRecord)rec.GetElement("@RNAM");
                race = race?.overrides.LastOrDefault() ?? race;
                if (race == null) continue;
                var female = rec.GetFlag(@"ACBS\Flags", "Female");
                var morphs = race.GetElement(female
                    ? "Female Face Morphs"
                    : "Male Face Morphs") as Container;
                if (morphs?.elements == null) continue;
                var index = Convert.ToUInt32(morph.data);
                foreach (var item in morphs.elements) {
                    if (!(item is Container entry)) continue;
                    if (entry.GetData<uint>("FMRI") != index) continue;
                    var nameElem = entry.GetElement("FMRN") as ValueElement;
                    var stringId =
                        (nameElem?.data as LocalizedString)?.id;
                    if (!stringId.HasValue) continue;
                    LoadSyntheticStrings(
                        plugin,
                        new Dictionary<uint, string> {
                            { stringId.Value, "TestMorph" },
                        });
                    var name = entry.GetValue("FMRN") ?? "";
                    Assert.AreEqual(
                        index.ToString("X8") + " " + name,
                        ((ValueDef)morph.def).formatDef.DataToValue(
                            morph, morph.data));
                    return;
                }
            }
            Assert.Ignore("No race-resolved face morph in scan cap.");
        }

        [Test]
        public void WiringMorphValue() {
            var session = NewSession();
            var plugin = LoadOrIgnore(session, "Fallout4.esm");
            var found = false;
            foreach (var rec in plugin.records
                .Where(r => r.signature.ToString() == "NPC_")
                .Take(500)) {
                var key = Walk(rec, "MorphValueFormat");
                if (key == null) continue;
                found = true;
                var text = ((ValueDef)key.def).formatDef.DataToValue(
                    key, key.data);
                Assert.That(
                    text,
                    Does.StartWith(
                        Convert.ToUInt32(key.data).ToString("X8")));
                break;
            }
            Assert.IsTrue(found, "No MorphValueFormat element decoded.");
        }

        [Test]
        public void MorphValueResolvedName() {
            var session = NewSession();
            var plugin = LoadOrIgnore(session, "Fallout4.esm");
            foreach (var rec in plugin.records
                .Where(r => r.signature.ToString() == "NPC_")
                .Take(2000)) {
                var key = Walk(rec, "MorphValueFormat");
                if (key == null) continue;
                var race = (MainRecord)rec.GetElement("@RNAM");
                race = race?.overrides.LastOrDefault() ?? race;
                if (race == null) continue;
                var female = rec.GetFlag(@"ACBS\Flags", "Female");
                var groups = race.GetElement(female
                    ? "Female Morph Groups"
                    : "Male Morph Groups") as Container;
                if (groups?.elements == null) continue;
                var index = Convert.ToUInt32(key.data);
                foreach (var item in groups.elements) {
                    if (!(item is Container group)) break;
                    var presets =
                        group.GetElement("Morph Presets") as Container;
                    if (presets?.elements == null) continue;
                    foreach (var presetItem in presets.elements) {
                        if (!(presetItem is Container preset)) continue;
                        if (preset.GetData<uint>("MPPI") != index) continue;
                        var groupName = group.GetValue("MPGN") ?? "";
                        var presetNameElem =
                            preset.GetElement("MPPN") as ValueElement;
                        var presetId =
                            (presetNameElem?.data as LocalizedString)?.id;
                        if (!presetId.HasValue) continue;
                        LoadSyntheticStrings(
                            plugin,
                            new Dictionary<uint, string> {
                                { presetId.Value, "TestPreset" },
                            });
                        var name = groupName +
                            " - " + (preset.GetValue("MPPN") ?? "");
                        Assert.AreEqual(
                            index.ToString("X8") + " " + name,
                            ((ValueDef)key.def).formatDef.DataToValue(
                                key, key.data));
                        return;
                    }
                }
            }
            Assert.Ignore("No race-resolved morph value in scan cap.");
        }
    }
}
