using esper.elements;
using esper.helpers;
using esper.resolution;
using esper.setup;

namespace esper.defs.FNV {
    public class CTDAParam2QuestObjectiveFormat : FormatDef {
        public static readonly string defId = "CTDAParam2QuestObjectiveFormat";

        internal CTDAParam2QuestObjectiveFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        internal override string DataToValue(ValueElement element, dynamic data) {
            var rec = element?.GetElement(@"..\..\CIS1") as MainRecord;
            var quest = rec?.referencedRecord;
            if (quest == null) return data.ToString();
            var index = (int)data;
            var stage = quest.GetElements("Objectives").FirstOrDefault(o =>
                o.GetData<int>(@"QOBJ\Objective Index") == index);
            if (stage == null)
                return $"{data} (Objective #{data} in \"{quest.GetValue("EDID")}\")";
            return $"{index:D3} {stage.GetValue(@"NNAM\Short Description")}";
        }

        internal override dynamic ValueToData(ValueElement element, string value) {
            return DataHelpers.ParseLeadingUInt(value);
        }
    }
}
