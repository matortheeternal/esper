using esper.elements;
using esper.helpers;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace esper.defs {
    public class EnumDef : FormatDef {
        public static Regex unknownOptionExpr = new Regex(@"^<(?:Unknown )?(-?\d+)>$");
        public static string defType = "enum"; 

        public Dictionary<string, string> options;
        public string unknownOption;

        public EnumDef(DefinitionManager manager, JObject src)
            : base(manager, src) {
            options = JsonHelpers.Dictionary<string, string>(src, "options");
            unknownOption = src.Value<string>("unknownOption");
        }

        public override string DataToValue(ValueElement element, dynamic data) {
            var options = this.options;
            var indexKey = data.ToString();
            if (!options.ContainsKey(indexKey)) 
                return unknownOption ?? string.Format("<Unknown {0}>", indexKey);
            return options[indexKey];
        }

        public override dynamic ValueToData(ValueElement element, string value) {
            var options = this.options;
            foreach (var (key, option) in options)
                if (option == value) 
                    return StringHelpers.DynamicParse(key);
            var match = unknownOptionExpr.Match(value);
            if (!match.Success) throw new Exception("Invalid option " + value);
            return StringHelpers.DynamicParse(match.Captures[1].Value);
        }
    }
}
