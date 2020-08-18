using esper.helpers;
using esper.setup;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;

namespace esper.defs {
    public class CTDAFunctions : Def {
        public static string defType = "ctdaFunctions";

        public ReadOnlyCollection<CTDAFunction> ctdaFunctions;

        public CTDAFunctions(DefinitionManager manager, JObject src) 
            : base(manager, src) {
            ctdaFunctions = JsonHelpers.Defs<CTDAFunction>(manager, src, "ctdaFunctions");
        } 
    }
}
