using esper.elements;
using esper.data;
using esper.plugins;
using esper.setup;
using System;
using Newtonsoft.Json.Linq;

namespace esper.defs {
    public class FormIdDef : ValueDef {
        public static readonly string defType = "formId";
        public override int? size => 4;

        public FormIdDef(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        public override dynamic ReadData(PluginFileSource source, UInt16? dataSize) {
            UInt32 data = source.reader.ReadUInt32();
            return FormId.FromSource(source.plugin, data);
        }

        public override dynamic DefaultData() {
            return new FormId(null, 0);
        }

        public override string GetValue(ValueElement element) {
            FormId data = element.data;
            return data.ToString();
        }

        public override void SetValue(ValueElement element, string value) {
            SetData(element, FormId.Parse(element, value));
        }

        public override string DataToSortKey(dynamic data) {
            var fid = (FormId)data;
            if (fid == null) return new string('0', 8);
            return fid.fileFormId.ToString("X8");
        }

        internal override void WriteData(
            ValueElement element, PluginFileOutput output
        ) {
            if (element.data is FormId data) {
                data.WriteTo(output);
            } else {
                DefaultData().WriteTo(output);
            }
        }
    }
}
