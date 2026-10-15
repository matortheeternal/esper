using esper.elements;
using esper.helpers;
using esper.resolution;
using esper.setup;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace esper.defs.FNV {
    public class CTDAParam2QuestObjectiveFormat : FormatDef {
        public static readonly string defId = "CTDAParam2QuestObjectiveFormat";
        public override bool customSortKey => true;

        internal CTDAParam2QuestObjectiveFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        private static MainRecord ResolveQuest(ValueElement element) {
            var param1 = element?.GetElement(@"..\Parameter #1");
            var paramValue = param1 as ValueElement
                ?? (param1 as Container)?.elements?.FirstOrDefault() as ValueElement;
            if (paramValue?.def is not esper.defs.FormIdDef) return null;
            var quest = paramValue.referencedRecord;
            if (quest == null) return null;
            var target = quest.master ?? quest;
            return target.overrides.LastOrDefault() ?? target;
        }

        internal override string DataToValue(ValueElement element, ValueData data) {
            var raw = data.ToDisplayString();
            var quest = ResolveQuest(element);
            if (quest == null)
                return $"{raw} <Warning: Could not resolve Parameter 1>";
            var shortName = quest.GetValue("EDID") ?? quest.signature.ToString();
            if (quest.signature.ToString() != "QUST")
                return $"{raw} <Warning: \"{shortName}\" is not a Quest record>";
            var index = (int)data.ToInt64();
            var stage = quest.GetElements("Objectives")?.FirstOrDefault(
                o => o.GetData<int?>(@"QOBJ") == index
            );
            if (stage == null)
                return $"{raw} <Warning: Quest Objective not found in \"{shortName}\">";
            var name = (stage.GetValue("NNAM") ?? "").Trim();
            return name.Length == 0 ? $"{index:D3}" : $"{index:D3} {name}";
        }

        internal override string GetSortKey(ValueElement element, ValueData data) {
            return ((int)data.ToInt64()).ToString("X8");
        }

        internal override ValueData ValueToData(ValueElement element, string value) {
            return DataHelpers.ParseLeadingUInt(value);
        }
    }
}
