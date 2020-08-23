using esper.elements;
using esper.resolution;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs.TES5 {
    public class CTDAParamStringFormat : FormatDef {
        public virtual string path => throw new NotImplementedException();

        public CTDAParamStringFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        public override string DataToValue(ValueElement element, dynamic data) {
            var container = element?.container;
            if (container == null) return "";
            return container.GetValue(path);
        }

        public override dynamic ValueToData(ValueElement element, string value) {
            // TODO: element?.container.SetValue(path, value);
            return 0;
        }
    }

    public class CTDAParam1StringFormat : CTDAParamStringFormat {
        public static readonly string defType = "CTDAParam1StringFormat";
        public override string path => @"..\CIS1";

        public CTDAParam1StringFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }
    }

    public class CTDAParam2StringFormat : CTDAParamStringFormat {
        public static readonly string defType = "CTDAParam2StringFormat";
        public override string path => @"..\CIS2";

        public CTDAParam2StringFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }
    }
}
