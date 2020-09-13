using esper.elements;
using esper.helpers;
using esper.resolution;
using esper.setup;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;

namespace esper.defs.TES5 {
    internal class TintEntries {
        public string raceId;
        public bool female;
        private Dictionary<int, string> entries;
        private string genderStr => female ? "Female" : "Male";
        private string tintMasksPath => 
            @$"Head Data\{genderStr} Head Data\Tint Masks";

        public TintEntries(MainRecord race, string raceId, bool female) {
            this.raceId = raceId;
            this.female = female;
            LoadEntries(race);
        }

        private string GetTintName(Element textureElement) {
            var tinp = textureElement.GetValue("TINP");
            if (tinp == null || tinp == "") return "";
            var tintFilePath = textureElement.GetValue("TINT");
            var tintName = Path.GetFileNameWithoutExtension(tintFilePath);
            return $"[{tinp}] {tintName}";
        }

        private void LoadEntries(MainRecord race) {
            var tintMasks = (Container) race.GetElement(tintMasksPath);
            if (tintMasks == null) return;
            entries = new Dictionary<int, string>(tintMasks.count);
            foreach (Element entry in tintMasks.elements) {
                var textureElement = entry.GetElement(@"Tint Layer\Texture");
                var index = textureElement.GetData("TINI");
                entries[index] = GetTintName(textureElement);
            }
        }

        public string GetEntryName(int index) {
            if (!entries.ContainsKey(index)) return null;
            return entries[index];
        }
    }

    internal class TintCache {
        private readonly List<TintEntries> cache = new List<TintEntries>();

        public TintEntries Get(MainRecord race, bool? female) {
            if (race == null || female == null) return null;
            var raceId = race.editorId;
            var entries = cache.Find(e => {
                return e.raceId == raceId && e.female == female;
            });
            if (entries == null) {
                entries = new TintEntries(race, raceId, (bool)female);
                cache.Add(entries);
            }
            return entries;
        }
    }

    public class TintLayerFormat : FormatDef {
        public static readonly string defId = "TintLayerFormat";
        private static readonly TintCache tintCache = new TintCache();

        public TintLayerFormat(DefinitionManager manager, JObject src)
            : base(manager, src) {}

        // TODO: sortKey, warnings

        public override string DataToValue(ValueElement element, dynamic data) {
            int index = data;
            var actor = element?.GetElement("^Record");
            var female = actor?.GetFlag(@"ACBS\Flags", "Female");
            var race = (MainRecord)actor?.GetElement("@RNAM");
            var tintEntries = tintCache.Get(race, female);
            if (tintEntries == null) return index.ToString();
            var entryName = tintEntries.GetEntryName(index) ??
                $"<Tint layer index not found in {race.name}>";
            return $"{index} {entryName}";
        }

        public override dynamic ValueToData(ValueElement element, string value) {
            return DataHelpers.ParseInt64(value);
        }
    }
}
