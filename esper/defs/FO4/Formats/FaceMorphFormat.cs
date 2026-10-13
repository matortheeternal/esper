using esper.elements;
using esper.resolution;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace esper.defs.FO4 {
    internal class FaceMorphCache : RaceIndexCache {
        protected override void LoadEntries(
            RaceIndexEntries entries, MainRecord race
        ) {
            var morphs = race.GetElement(
                entries.female ? "Female Face Morphs" : "Male Face Morphs"
            ) as Container;
            if (morphs?.elements == null) return;
            foreach (var item in morphs.elements) {
                if (!(item is Container entry)) continue;
                entries.entries[entry.GetData<uint>("FMRI")] =
                    Fo4FormatHelpers.SafeName(entry, "FMRN");
            }
        }
    }

    public class FaceMorphFormat : FormatDef {
        public static readonly string defId = "FaceMorphFormat";
        public override bool customSortKey => true;
        private static readonly FaceMorphCache cache = new FaceMorphCache();

        internal FaceMorphFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        internal override string DataToValue(ValueElement element, dynamic data) {
            var index = Convert.ToUInt32(data);
            var hex = index.ToString("X8");
            var actor = element?.record;
            var female = actor?.GetFlag(@"ACBS\Flags", "Female");
            var race = (MainRecord)actor?.GetElement("@RNAM");
            race = race?.overrides.LastOrDefault() ?? race;
            var found = cache.Get(race, female);
            if (found == null) return hex;
            var name = found.GetEntryName(index);
            if (string.IsNullOrEmpty(name))
                name = "<Face morph index not found in " + race.name + ">";
            return hex + " " + name;
        }

        internal override string GetSortKey(ValueElement element, dynamic data) {
            return Convert.ToUInt32(data).ToString("X8");
        }

        internal override dynamic ValueToData(ValueElement element, string value) {
            return Fo4FormatHelpers.WbHexStrToInt(value);
        }
    }
}
