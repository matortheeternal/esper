using esper.elements;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace esper.defs {
    public class FlagsDef : FormatDef {
        public static Regex unknownFlagExpr = new Regex(@"^Unknown (\d+)$");

        public static string defType = "flags";

        public JObject flags => src.Value<JObject>("flags");

        public FlagsDef(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) {}

        public string GetFlagValue(int index) {
            var flags = this.flags;
            var indexKey = index.ToString();
            if (!flags.ContainsKey(indexKey)) return "Unknown " + index;
            return flags.Value<string>(indexKey);
        }

        public int GetFlagIndex(string flag) {
            var flags = this.flags;
            foreach (var (key, value) in flags)
                if (value.Value<string>() == flag) return int.Parse(key);
            Match match = unknownFlagExpr.Match(flag);
            if (!match.Success) return -1;
            return int.Parse(match.Captures[0].Value);
        }

        public List<string> DataToArray(ValueElement element, dynamic data) {
            var list = new List<string>();
            var numBits = 8 * element.valueDef.size;
            for (int i = 0; i < numBits; i++)
                if ((data & (1 << i)) != 0) list.Add(GetFlagValue(i));
            return list;
        }

        public override string DataToValue(ValueElement element, dynamic data) {
            return string.Join(", ", DataToArray(element, data));
        }

        public bool FlagIsSet(dynamic data, string flag) {
            var flagIndex = GetFlagIndex(flag);
            if (flagIndex == -1) return false;
            return (data & (1 << flagIndex)) != 0;
        }

        public override dynamic ValueToData(ValueElement element, string value) {
            // TODO
            throw new NotImplementedException();
        }
    }
}
