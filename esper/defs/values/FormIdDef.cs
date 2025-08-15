using esper.elements;
using esper.data;
using esper.io;
using esper.setup;
using System;
using Newtonsoft.Json.Linq;
using esper.helpers;
using esper.resolution;

namespace esper.defs {
    public class FormIdDef : ValueDef {
        public static readonly string defId = "formId";
        public override XEDefType valueDefType => XEDefType.dtIntegerFormater;
        public override SmashType smashType => SmashType.stInteger;

        internal readonly SignaturesDef allowedSignatures = null;
        internal readonly bool validateFlstRefs = false;
        internal readonly bool persistent = false;

        public override int? size => 4;
        public override bool canContainFormIds => true;

        public FormIdDef(DefinitionManager manager, JObject src)
            : base(manager, src) {
            if (src.ContainsKey("signatures"))
                allowedSignatures = JsonHelpers.SignaturesDef(manager, src);
            if (src.ContainsKey("validateFlstRefs"))
                validateFlstRefs = src.Value<bool>("validateFlstRefs");
            if (src.ContainsKey("persistent"))
                persistent = src.Value<bool>("persistent");
        }

        public override dynamic ReadData(DataSource source, UInt32? dataSize) {
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

        internal void ValidateFormListRefs(ValueElement element, MainRecord rec) {
            var formIdElements = rec.GetElements("FormIDs");
            if (formIdElements == null) return;
            foreach (ValueElement v in formIdElements) { 
                var fid = v?.data as FormId;
                var entryRec = fid?.ResolveRecord();
                if (entryRec == null) continue;
                var sig = entryRec.signature.ToString();
                if (sig == "NULL" || sig != "FLST" || !allowedSignatures.Contains(sig))
                    throw new Exception(
                        $"{element.fullPath} does not allow form lists with " +
                        $"references to {sig} records."
                    );
            }
        }

        public void ValidateRef(ValueElement element, MainRecord rec) {
            var sig = rec.signature.ToString();
            if (!allowedSignatures.Contains(sig))
                throw new Exception(
                    $"{element.fullPath} does not allow references " +
                    $"with signature {sig}."
                );
            if (persistent && !rec.GetRecordFlag("Persistent"))
                throw new Exception(
                    $"{element.fullPath} does not allow references to " +
                    "non-persistent records."
                );
            if (validateFlstRefs && sig == "FLST")
                ValidateFormListRefs(element, rec);
        }

        public override void SetValue(ValueElement element, string value) {
            var fid = FormId.Parse(element, value);
            if (sessionOptions.enforceExpectedReferences && allowedSignatures != null)
                ValidateRef(element, fid.ResolveRecord());
            SetData(element, fid);
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
