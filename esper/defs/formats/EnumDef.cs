using esper.elements;
using esper.helpers;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;
using System.Text.RegularExpressions;

namespace esper.defs {
    public class EnumDef : FormatDef {
        public static Regex unknownOptionExpr = new Regex(@"^<(?:Unknown )?(-?\d+)>$");
         
        public JObject options => src.Value<JObject>("options");
        public string unknownOption => src.Value<string>("unknownOption");

        public EnumDef(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) {}

        public new string DataToValue(ValueElement element, dynamic data) {
            var options = this.options;
            var indexKey = data.ToString();
            if (!options.ContainsKey(indexKey)) 
                return unknownOption ?? string.Format("<Unknown {0}>", indexKey);
            return options.Value<string>(indexKey);
        }

        public new dynamic ValueToData(ValueElement element, string value) {
            var options = this.options;
            foreach (var (key, option) in options)
                if (option.Value<string>() == value) 
                    return StringHelpers.DynamicParse(key);
            var match = unknownOptionExpr.Match(value);
            if (!match.Success) throw new Exception("Invalid option " + value);
            return StringHelpers.DynamicParse(match.Captures[1].Value);
        }
    }
}
