using esper.data;
using esper.elements;
using esper.helpers;
using esper.resolution;
using esper.setup;
using System.Collections.ObjectModel;

namespace esper.defs.TES5 {
    public class AliasFormat : FormatDef {
        internal AliasFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        public virtual MainRecord ResolveQuestRec(ValueElement element) {
            throw new NotImplementedException();
        }

        private string DataToValue(ValueData data) {
            if (data.ToInt64() == -1) return "None";
            return data.ToDisplayString();
        }

        private List<Element> GetAliases(MainRecord questRef) {
            return questRef.GetElements("Aliases");
        }

        private string AliasToStr(Element alias) {
            int index = alias.GetData<int>("[0]");
            string alid = alias.GetValue("Alias Name");
            return alid != "" ? $"{index,3} {alid}" : $"{index,3}";
        }

        // TODO: warnings
        // TODO: sort key
        // TODO: edit info?

        internal override string DataToValue(ValueElement element, ValueData data) {
            if (!sessionOptions.resolveAliases) return DataToValue(data);
            var questRef = ResolveQuestRec(element);
            if (questRef == null || questRef.signature != Signatures.QUST)
                return DataToValue(data);
            foreach (Element alias in GetAliases(questRef))
                if (alias.GetData<long>("[0]") == data.ToInt64()) return AliasToStr(alias);
            return DataToValue(data);
        }

        internal override ValueData ValueToData(ValueElement element, string value) {
            return DataHelpers.ParseInt64(value, -1);
        }
    }

    public class ConditionAliasFormat : AliasFormat {
        public static readonly string defId = "ConditionAliasFormat";

        internal ConditionAliasFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        public override MainRecord ResolveQuestRec(ValueElement element) {
            var rec = element.record;
            return (rec.signature.ToString() switch {
                "QUST" => rec,
                "SCEN" => rec.GetElement("@PNAM"),
                "PACK" => rec.GetElement("@QNAM"),
                "INFO" => rec.GetElement(@"..\..\@QNAM"),
                _ => null
            }) as MainRecord;
        }
    }

    public class PackageLocationAliasFormat : AliasFormat {
        public static readonly string defId = "PackageLocationAliasFormat";

        internal PackageLocationAliasFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        public override MainRecord ResolveQuestRec(ValueElement element) {
            return element?.record?.GetElement("@QNAM") as MainRecord;
        }
    }

    public class QuestAliasFormat : AliasFormat {
        public static readonly string defId = "QuestAliasFormat";

        internal QuestAliasFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        public override MainRecord ResolveQuestRec(ValueElement element) {
            return element.record;
        }
    }

    public class QuestExternalAliasFormat : AliasFormat {
        public static readonly string defId = "QuestExternalAliasFormat";

        internal QuestExternalAliasFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        public override MainRecord ResolveQuestRec(ValueElement element) {
            return element.GetElement(@"..\@ALEQ") as MainRecord;
        }
    }

    public class ScriptObjectAliasFormat : AliasFormat {
        public static readonly string defId = "ScriptObjectAliasFormat";

        internal ScriptObjectAliasFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        public override MainRecord ResolveQuestRec(ValueElement element) {
            return (MainRecord)element?.GetElement(@"..\@FormID");
        }
    }
}
