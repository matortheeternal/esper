using esper.data;
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

        public JObject flags {
            get => src.Value<JObject>("flags");
        }

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
            if (match == null) throw new Exception("Invalid flag " + flag);
            return int.Parse(match.Captures[0].Value);
        }

        public List<string> DataToArray(Element element, DataContainer data) {
            // TODO
            throw new NotImplementedException();
        }

        public new string DataToValue(Element element, DataContainer data) {
            return string.Join(", ", DataToArray(element, data));
        }

        public new DataContainer ValueToData(Element element, string value) {
            // TODO
            throw new NotImplementedException();
        }
    }
}
