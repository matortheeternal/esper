using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs {
    public class CTDAFunction : Def {
        public static string defType = "ctdaFunction";

        public UInt16 index;
        public string name;
        public CTDAFunctionParamType? paramType1;
        public CTDAFunctionParamType? paramType2;
        public CTDAFunctionParamType? paramType3;

        public CTDAFunction(DefinitionManager manager, JObject src)
            : base(manager, src) {
            index = src.Value<UInt16>("index");
            name = src.Value<string>("name");
            paramType1 = ParseParamType(src, "paramType1");
            paramType2 = ParseParamType(src, "paramType2");
            paramType3 = ParseParamType(src, "paramType3");
        }

        private CTDAFunctionParamType? ParseParamType(JObject src, string key) {
            var value = src.Value<string>(key);
            if (value == null) return null;
            return (CTDAFunctionParamType) Enum.Parse(
                typeof(CTDAFunctionParamType), 
                value
            );
        }
    }
}
