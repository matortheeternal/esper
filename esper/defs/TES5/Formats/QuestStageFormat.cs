using esper.data;
using esper.elements;
using esper.helpers;
using esper.resolution;
using esper.setup;
using esper.warnings;

namespace esper.defs.TES5 {
    public class QuestStageFormat : FormatDef {
        internal QuestStageFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        private Element GetMatchingStage(MainRecord rec, int index) {
            return rec.GetElements("Stages").FirstOrDefault(stage => {
                return stage.GetData<int>(@"INDX\Stage Index") == index;
            });
        }

        internal virtual MainRecord GetQuest(ValueElement element) {
            throw new NotImplementedException();
        }

        public bool GetWarnings(
            ValueElement element, ElementWarnings warnings
        ) {
            var rec = GetQuest(element);
            if (rec == null)
                return warnings.Add(element, "Could not resolve Parameter 1");
            if (rec.signature != Signatures.QUST)
                return warnings.Add(element, $"{rec.name} is not a Quest record");
            var stage = GetMatchingStage(rec, (int)element._data.ToInt64());
            if (stage == null)
                return warnings.Add(element, $"Quest Stage not found in {rec.name}");
            return false;
        }

        private string StageToValue(Element stage) {
            var index = stage.GetData<int>(@"INDX\Stage Index");
            var cnam = stage.GetValue(@"Log Entries\Log Entry\CNAM");
            return cnam != null
                ? $"{index:D3} {cnam}"
                : $"{index:D3}";
        }

        internal override string DataToValue(ValueElement element, ValueData data) {
            var rec = GetQuest(element);
            if (rec == null || rec.signature != Signatures.QUST) return data.ToDisplayString();
            var stage = GetMatchingStage(rec, (int)data.ToInt64());
            return stage != null
                ? StageToValue(stage)
                : data.ToDisplayString();
        }

        internal override ValueData ValueToData(ValueElement element, string value) {
            return DataHelpers.ParseLeadingUInt(value);
        }
    }

    public class PerkDATAQuestStageFormat : QuestStageFormat {
        public static readonly string defId = "PerkDATAQuestStageFormat";

        internal PerkDATAQuestStageFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        internal override MainRecord GetQuest(ValueElement element) {
            return element?.container?.GetElement("@Quest") as MainRecord;
        }
    }

    public class CTDAParam2QuestStageFormat : QuestStageFormat {
        public static readonly string defId = "CTDAParam2QuestStageFormat";

        internal CTDAParam2QuestStageFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        internal override MainRecord GetQuest(ValueElement element) {
            return element?.container?.GetElement("@Parameter #1") as MainRecord;
        }
    }
}
