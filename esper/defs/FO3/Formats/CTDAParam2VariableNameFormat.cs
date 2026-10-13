using esper.elements;
using esper.helpers;
using esper.resolution;
using esper.setup;

namespace esper.defs.FO3 {
    public class CTDAParam2VariableNameFormat : FormatDef {
        public static readonly string defId = "CTDAParam2VariableNameFormat";

        internal CTDAParam2VariableNameFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        private static MainRecord ResolveScript(ValueElement element) {
            var rec = element?.container?.GetElement("@Parameter #1") as MainRecord;
            if (rec == null) return null;
            var target = rec.master ?? rec;
            var win = target.overrides.LastOrDefault() ?? target;
            var scri = win.GetElement("SCRI");
            if (scri?.referencedRecord is not MainRecord script) return null;
            var scriptTarget = script.master ?? script;
            return scriptTarget.overrides.LastOrDefault() ?? scriptTarget;
        }

        private static string FindVariable(MainRecord script, int index) {
            var locals = script?.GetElements("Local Variables");
            if (locals == null) return null;
            foreach (var localVar in locals) {
                if (localVar.GetData<int>(@"SLSD\Index") == index)
                    return localVar.GetValue("SCVR");
            }
            return null;
        }

        internal override string DataToValue(ValueElement element, dynamic data) {
            var index = element?.GetData<int?>() ?? 0;
            var name = FindVariable(ResolveScript(element), index);
            return name ?? data.ToString();
        }

        internal override dynamic ValueToData(ValueElement element, string value) {
            if (long.TryParse(value.Trim(), out var number)) return number;
            var script = ResolveScript(element);
            var locals = script?.GetElements("Local Variables");
            if (locals != null) {
                foreach (var localVar in locals) {
                    var name = localVar.GetValue("SCVR");
                    if (string.Equals(name, value.Trim(), System.StringComparison.OrdinalIgnoreCase))
                        return localVar.GetData<int>(@"SLSD\Index");
                }
            }
            return DataHelpers.ParseInt64(value);
        }
    }
}
