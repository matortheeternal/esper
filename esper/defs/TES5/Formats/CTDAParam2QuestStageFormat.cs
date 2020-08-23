using esper.elements;
using esper.helpers;
using esper.resolution;
using esper.setup;
using esper.warnings;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace esper.defs.TES5 {
    public class CTDAParam2QuestStageFormat : FormatDef {
        public static string defType = "CTDAParam2QuestStageFormat";

        public CTDAParam2QuestStageFormat(DefinitionManager manager, JObject src)
            : base(manager, src) {}

        private Element GetMatchingStage(MainRecord rec, int index) {
            return rec.GetElements("Stages").FirstOrDefault(stage => {
                return stage.GetData(@"INDX\Stage Index") == index;
            });
        }

        public bool GetWarnings(
            ValueElement element, ElementWarnings warnings
        ) {
            var rec = element?.container?.GetElement("@Parameter #1") as MainRecord;
            if (rec == null) 
                return warnings.Add(element, "Could not resolve Parameter 1");
            if (rec.signature != "QUST") 
                return warnings.Add(element, $"{rec.name} is not a Quest record");
            var stage = GetMatchingStage(rec, (int) element._data);
            if (stage == null)
                return warnings.Add(element, $"Quest Stage not found in {rec.name}");
            return false;
        }

        private string StageToValue(Element stage) {
            var index = stage.GetData(@"INDX\Stage Index");
            var cnam = stage.GetValue(@"Log Entries\Log Entry\CNAM");
            return cnam != null
                ? $"{index:D3} {cnam}"
                : $"{index:D3}";
        }

        public override string DataToValue(ValueElement element, dynamic data) {
            var rec = element?.container?.GetElement("@Parameter #1") as MainRecord;
            if (rec == null || rec.signature != "QUST") return data.ToString();
            var stage = GetMatchingStage(rec, (int) data);
            return stage != null
                ? StageToValue(stage)
                : data.ToString();
        }

        public override dynamic ValueToData(ValueElement element, string value) {
            return DataHelpers.ParseLeadingUInt(value);
        }
    }
}
