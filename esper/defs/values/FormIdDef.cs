using esper.elements;
using esper.plugins;
using esper.parsing;
using esper.setup;
using System;
using Newtonsoft.Json.Linq;

namespace esper.defs {
    public class FormIdDef : ValueDef {
        public static string defType = "formId";
        public new int size { get => 4; }

        public FormIdDef(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) { }

        public override dynamic ReadData(PluginFileSource source) {
            UInt32 data = source.reader.ReadUInt32();
            byte ordinal = (byte)(data >> 24);
            var targetPlugin = source.plugin.OrdinalToFile(ordinal, false);
            var formId = data & 0xFFFFFF;
            return new FormId(targetPlugin, formId);
        }

        public override dynamic DefaultData() {
            return new FormId(null, 0);
        }

        public override string GetValue(ValueElement element) {
            FormId data = element.data;
            return data.ToString();
        }

        public override void SetValue(ValueElement element, string value) {
            // TODO: make form ID parsers
        }
    }
}
