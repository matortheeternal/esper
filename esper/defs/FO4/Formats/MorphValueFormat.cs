using esper.elements;
using esper.resolution;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace esper.defs.FO4 {
    internal class MorphValueCache : RaceIndexCache {
        protected override void LoadEntries(
            RaceIndexEntries entries, MainRecord race
        ) {
            var groups = race.GetElement(
                entries.female ? "Female Morph Groups" : "Male Morph Groups"
            ) as Container;
            if (groups?.elements != null) {
                foreach (var item in groups.elements) {
                    if (!(item is Container group)) break;
                    var groupName =
                        Fo4FormatHelpers.SafeName(group, "MPGN");
                    var presets =
                        group.GetElement("Morph Presets") as Container;
                    if (presets?.elements == null) continue;
                    foreach (var presetItem in presets.elements) {
                        if (!(presetItem is Container preset)) continue;
                        entries.entries[preset.GetData<uint>("MPPI")] =
                            groupName + " - " +
                            Fo4FormatHelpers.SafeName(preset, "MPPN");
                    }
                }
            }
            var values = race.GetElement("Morph Values") as Container;
            if (values?.elements == null) return;
            foreach (var item in values.elements) {
                if (!(item is Container morph)) continue;
                entries.entries[morph.GetData<uint>("MSID")] =
                    Fo4FormatHelpers.SafeName(morph, "MSM0") + "/" +
                    Fo4FormatHelpers.SafeName(morph, "MSM1");
            }
        }
    }

    public class MorphValueFormat : FormatDef {
        public static readonly string defId = "MorphValueFormat";
        public override bool customSortKey => true;
        private static readonly MorphValueCache cache = new MorphValueCache();

        internal MorphValueFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        internal override string DataToValue(ValueElement element, ValueData data) {
            var index = Convert.ToUInt32(data.ToInt64());
            var hex = index.ToString("X8");
            var actor = element?.record;
            var female = actor?.GetFlag(@"ACBS\Flags", "Female");
            var race = (MainRecord)actor?.GetElement("@RNAM");
            race = race?.overrides.LastOrDefault() ?? race;
            var found = cache.Get(race, female);
            if (found == null) return hex;
            var name = found.GetEntryName(index);
            if (string.IsNullOrEmpty(name))
                name = "<Morph index not found in " + race.name + ">";
            return hex + " " + name;
        }

        internal override string GetSortKey(ValueElement element, ValueData data) {
            return Convert.ToUInt32(data.ToInt64()).ToString("X8");
        }

        internal override ValueData ValueToData(ValueElement element, string value) {
            return Fo4FormatHelpers.WbHexStrToInt(value);
        }
    }
}
