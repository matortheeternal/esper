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
            if (!sessionOptions.resolveAliases) return data.ToString();
            var questRef = ResolveQuestRec(element);
            if (questRef == null || questRef.signature != "QUST") 
                return DataToValue(data);
            foreach (Element alias in GetAliases(questRef))
                if (alias.GetData("[0]") == data) return AliasToStr(alias);
            return DataToValue(data);
        }

        public override dynamic ValueToData(ValueElement element, string value) {
            if (value == "None") return -1; // not necessary?
            return DataHelpers.ParseInt64(value, -1);
        }
    }
}
