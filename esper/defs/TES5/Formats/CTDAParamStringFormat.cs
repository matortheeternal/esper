using esper.elements;
using esper.resolution;
using esper.setup;

namespace esper.defs.TES5 {
    public class CTDAParamStringFormat : FormatDef {
        internal CTDAParamStringFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        internal virtual ValueElement GetParamElement(ValueElement element) {
            throw new NotImplementedException();
        }

        internal override string DataToValue(ValueElement element, VariantData data) {
            var paramElement = GetParamElement(element);
            return paramElement?.value ?? "";
        }

        internal override VariantData ValueToData(ValueElement element, string value) {
            var paramElement = GetParamElement(element);
            if (paramElement != null) paramElement.value = value;
            return 0;
        }
    }

    public class CTDAParam1StringFormat : CTDAParamStringFormat {
        public static readonly string defId = "CTDAParam1StringFormat";

        internal CTDAParam1StringFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        internal override ValueElement GetParamElement(ValueElement element) {
            return element?.GetElement(@"..\..\CIS1") as ValueElement;
        }
    }

    public class CTDAParam2StringFormat : CTDAParamStringFormat {
        public static readonly string defId = "CTDAParam2StringFormat";

        internal CTDAParam2StringFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        internal override ValueElement GetParamElement(ValueElement element) {
            return element?.GetElement(@"..\..\CIS2") as ValueElement;
        }
    }
}
