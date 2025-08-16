using esper.helpers;
using esper.setup;

namespace esper.defs {
    [JSExport]
    public class CTDAFunctions : Def {
        public static readonly string defId = "ctdaFunctions";

        public List<CTDAFunction> ctdaFunctions;

        public CTDAFunctions(DefinitionManager manager, JObject src) 
            : base(manager, src) {
            ctdaFunctions = JsonHelpers.Defs<CTDAFunction>(manager, src, "ctdaFunctions");
        }

        public CTDAFunction FunctionByIndex(UInt16 index) {
            return CollectionHelpers.BinarySearch(ctdaFunctions, fn => {
                return index.CompareTo(fn.index);
            });
        }

        public CTDAFunction FunctionByName(string name) {
            return ctdaFunctions.FirstOrDefault(fn => {
                return fn.name == name;
            });
        }
    }
}
