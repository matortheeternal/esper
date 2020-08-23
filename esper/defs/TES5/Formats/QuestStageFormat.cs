using esper.elements;
using esper.helpers;
using esper.resolution;
using esper.setup;
using esper.warnings;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace esper.defs.TES5 {
    public class QuestStageFormat : FormatDef {
        public virtual string questRefPath => throw new NotImplementedException();

        public QuestStageFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        private Element GetMatchingStage(MainRecord rec, int index) {
            return rec.GetElements("Stages").FirstOrDefault(stage => {
                return stage.GetData(@"INDX\Stage Index") == index;
            });
        }

        private MainRecord GetQuest(ValueElement element) {
            return element?.container?.GetElement(questRefPath) as MainRecord;
        }

        public bool GetWarnings(
            ValueElement element, ElementWarnings warnings
        ) {
            var rec = GetQuest(element);
            if (rec == null)
                return warnings.Add(element, "Could not resolve Parameter 1");
            if (rec.signature != "QUST")
                return warnings.Add(element, $"{rec.name} is not a Quest record");
            var stage = GetMatchingStage(rec, (int)element._data);
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
            var rec = GetQuest(element);
            if (rec == null || rec.signature != "QUST") return data.ToString();
            var stage = GetMatchingStage(rec, (int)data);
            return stage != null
                ? StageToValue(stage)
                : data.ToString();
        }

        public override dynamic ValueToData(ValueElement element, string value) {
            return DataHelpers.ParseLeadingUInt(value);
        }
    }

    public class PerkDATAQuestStageFormat : QuestStageFormat {
        public static readonly string defType = "PerkDATAQuestStageFormat";
        public override string questRefPath => "@Quest";

        public PerkDATAQuestStageFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }
    }

    public class CTDAParam2QuestStageFormat : QuestStageFormat {
        public static readonly string defType = "CTDAParam2QuestStageFormat";
        public override string questRefPath => "@Parameter #1";

        public CTDAParam2QuestStageFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }
    }
}
