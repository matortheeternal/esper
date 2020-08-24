using esper.elements;
using esper.helpers;
using esper.resolution;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace esper.defs.TES5 {
    public class AliasFormat : FormatDef {
        public AliasFormat(DefinitionManager manager, JObject src) 
            : base(manager, src) { }

        public virtual MainRecord ResolveQuestRec(ValueElement element) {
            throw new NotImplementedException();
        }

        private string DataToValue(dynamic data) {
            if (data == -1) return "None";
            return data.ToString();
        }

        private List<Element> GetAliases(MainRecord questRef) {
            return questRef.GetElements("Aliases");
        }

        private string AliasToStr(Element alias) {
            int index = alias.GetData("[0]");
            string alid = alias.GetValue("ALID");
            return alid != "" ? $"{index:3} {alid}" : $"{index:3}";
        }

        // TODO: warnings
        // TODO: sort key
        // TODO: edit info?

        public override string DataToValue(ValueElement element, dynamic data) {
            if (!sessionOptions.resolveAliases) return DataToValue(data);
            var questRef = ResolveQuestRec(element);
            if (questRef == null || questRef.signature != "QUST") 
                return DataToValue(data);
            foreach (Element alias in GetAliases(questRef))
                if (alias.GetData("[0]") == data) return AliasToStr(alias);
            return DataToValue(data);
        }

        public override dynamic ValueToData(ValueElement element, string value) {
            return DataHelpers.ParseInt64(value, -1);
        }
    }

    public class ConditionAliasFormat : AliasFormat {
        public static readonly string defType = "ConditionAliasFormat";

        public ConditionAliasFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        public override MainRecord ResolveQuestRec(ValueElement element) {
            var rec = element.record;
            return (rec.signature switch {
                "QUST" => rec,
                "SCEN" => rec.GetElement("@PNAM"),
                "PACK" => rec.GetElement("@QNAM"),
                "INFO" => rec.GetElement(@"..\..\@QNAM"),
                _ => null
            }) as MainRecord;
        }
    }

    public class PackageLocationAliasFormat : AliasFormat {
        public static readonly string defType = "PackageLocationAliasFormat";

        public PackageLocationAliasFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        public override MainRecord ResolveQuestRec(ValueElement element) {
            return element?.record?.GetElement("@QNAM") as MainRecord;
        }
    }

    public class QuestAliasFormat : AliasFormat {
        public static readonly string defType = "QuestAliasFormat";

        public QuestAliasFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        public override MainRecord ResolveQuestRec(ValueElement element) {
            return element.record;
        }
    }

    public class QuestExternalAliasFormat : AliasFormat {
        public static readonly string defType = "QuestExternalAliasFormat";

        public QuestExternalAliasFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        public override MainRecord ResolveQuestRec(ValueElement element) {
            return element.GetElement(@"..\@ALEQ") as MainRecord;
        }
    }

    public class ScriptObjectAliasFormat : AliasFormat {
        public static readonly string defType = "ScriptObjectAliasFormat";

        public ScriptObjectAliasFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        public override MainRecord ResolveQuestRec(ValueElement element) {
            return (MainRecord)element?.GetElement(@"..\@FormID");
        }
    }
}
