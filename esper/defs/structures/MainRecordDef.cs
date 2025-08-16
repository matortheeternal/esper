using esper.data;
using esper.elements;
using esper.helpers;
using esper.plugins;
using esper.resolution;
using esper.setup;
using esper.io;

namespace esper.defs {
    [JSExport]
    public class MainRecordDef : MembersDef {
        public static readonly string defId = "record";
        public override string description => $"MainRecord <{signature}>";
        public override XEDefType defType => XEDefType.dtRecord;
        public override SmashType smashType => SmashType.stRecord;
        public override bool canContainFormIds => true;

        private readonly Signature _signature;
        private UInt32 recordHeaderSize => 24;

        public override Signature signature => _signature;
        public StructDef headerDef;
        public FormIdDef containedInDef;
        internal FlagsDef recordFlagsDef;

        public MainRecordDef(DefinitionManager manager, JObject src)
            : base(manager, src) {
            ErrorHelpers.CheckDefProperty(src, "signature");
            var sig = src.Value<string>("signature");
            _signature = Signature.FromString(sig);
            headerDef = BuildHeaderDef(src.Value<JObject>("flags"));
            containedInDef = (FormIdDef) JsonHelpers.Def(manager, src, "containedInElement");
            recordFlagsDef = GetRecordFlagsDef();
        }

        private FlagsDef GetRecordFlagsDef() {
            foreach (var def in headerDef.elementDefs)
                if (def.name == "Record Flags" && def is ValueDef valueDef)
                    if (valueDef.formatDef is FlagsDef flagsDef) 
                        return flagsDef;
            return null;
        }

        private StructDef BuildHeaderDef(JObject flagsSrc) {
            var baseHeaderDef = manager.recordHeaderDef;
            if (flagsSrc == null) return baseHeaderDef;
            var headerDef = new StructDef(baseHeaderDef);
            var elementDefs = headerDef.elementDefs.ToList();
            elementDefs[2] = new UInt32Def((UInt32Def)elementDefs[2]) {
                formatDef = (FormatDef)manager.BuildDef(flagsSrc)
            };
            headerDef.elementDefs = elementDefs;
            return headerDef;
        }

        private void ReadHeader(MainRecord rec, PluginFileSource source) {
            source.stream.Position = rec.bodyOffset - recordHeaderSize;
            rec.header.ToStructElement(rec, source);
        }

        internal void HandleSubrecord(
            MainRecord rec, RecordSource source, ref int defIndex
        ) {
            var subrecord = source.currentSubrecord;
            var def = GetMemberDef(subrecord.signature, ref defIndex);
            if (def == null) {
                rec._unexpectedSubrecords.Add(subrecord);
                source.SubrecordHandled();
            } else if (def.IsSubrecord()) {
                def.ReadElement(rec, source, subrecord.dataSize);
                source.SubrecordHandled();
                defIndex++;
            } else {
                var container = (Container)def.PrepareElement(rec);
                def.SubrecordFound(container, source);
                defIndex++;
            }
        }

        private void ReadSubrecords(MainRecord rec, RecordSource source) {
            int defIndex = 0;
            while (true) {
                if (!source.NextSubrecord()) break;
                HandleSubrecord(rec, source, ref defIndex);
            }
        }

        internal MainRecord GetParentRec(MainRecord rec) {
            GroupRecord group = rec.container as GroupRecord;
            if (group == null || !group.hasRecordParent) return null;
            return group.GetParentRecord();
        }

        internal void InitContainedInElements(MainRecord rec) {
            var parentRec = GetParentRec(rec);
            var containedInDef = parentRec?.mrDef.containedInDef;
            if (containedInDef == null) return;
            var element = (ValueElement)containedInDef.NewElement(rec);
            element._data = FormId.FromSource(parentRec._file, parentRec.fileFormId);
        }

        internal void UpdateContainedIn(MainRecord rec) {
            var parentRec = GetParentRec(rec);
            var containedInDef = parentRec?.mrDef.containedInDef;
            if (containedInDef == null) return;
            var element = (ValueElement) rec.FindElementForDef(containedInDef);
            element._data = FormId.FromSource(parentRec._file, parentRec.fileFormId);
        }

        internal void ReadElements(MainRecord rec, PluginFileSource source) {
            rec._internalElements = new List<Element>();
            rec._unexpectedSubrecords = new List<Subrecord>();
            InitContainedInElements(rec);
            ReadHeader(rec, source);
            source.WithRecordData(rec, () => {
                ReadSubrecords(rec, rec.recordSource);
                rec.ElementsReady();
            });
        }

        internal void InitElement(MainRecord rec) {
            foreach (var def in memberDefs) {
                if (!def.required) continue;
                if (rec._internalElements.Any(e => e.def == def)) continue;
                var e = def.NewElement(rec);
                e.Initialize();
            }
        }

        internal int GetFirstRealElementIndex(MainRecord rec) {
            var pRec = GetParentRec(rec);
            return pRec?.mrDef.containedInDef != null ? 2 : 1;
        }

        internal override UInt32 GetSize(Element element) {
            var rec = (MainRecord)element;
            if (rec._internalElements == null)
                return recordHeaderSize + rec.header.dataSize;
            int index = GetFirstRealElementIndex(rec);
            UInt32 size = 0;
            for (; index < rec.count; index++)
                size += rec._internalElements[index].size;
            return size;
        }

        internal void UpdateDataSize(Element headerElement, UInt32 newSize) {
            var element = (ValueElement) headerElement.GetElement("Data Size");
            element._data = newSize;
        }

        internal void WriteElementsTo(MainRecord rec, PluginFileOutput output) {
            int index = GetFirstRealElementIndex(rec);
            var headerElement = rec._internalElements[index - 1];
            UpdateDataSize(headerElement, rec.size);
            headerElement.WriteTo(output);
            output.WriteRecordData(rec, () => {
                for (; index < rec._internalElements.Count; index++)
                    rec._internalElements[index].WriteTo(output);
            });
        }

        internal void WriteSourceTo(MainRecord rec, PluginFileOutput output) {
            var source = rec._file.source;
            source.stream.Position = rec.bodyOffset - recordHeaderSize;
            int size = (int)(recordHeaderSize + rec.header.dataSize);
            source.PipeTo(output.writer, size);
        }

        internal override void WriteElement(
            Element element, PluginFileOutput output
        ) {
            var rec = (MainRecord) element;
            var file = rec._file;
            if (rec._internalElements == null) {
                if (!(file as IMasterManager).mastersChanged) {
                    WriteSourceTo(rec, output);
                    return;
                }
                ReadElements(rec, file.source);
            }
            WriteElementsTo(rec, output);
        }

        internal bool RecordFlagIsSet(MainRecord rec, string flag) {
            if (recordFlagsDef == null) 
                return rec.GetFlag(@"Record Header\Record Flags", flag);
            return recordFlagsDef.FlagIsSet(rec.header.flags, flag);
        }

        internal bool RecordFlagIsSet(MainRecord rec, int flagIndex) {
            return (rec.header.flags & (uint)1 << flagIndex) != 0;
        }

        internal override JObject ToJObject(bool isBase = false) {
            var src = base.ToJObject(isBase);
            if (isBase) src.Add("type", "record");
            return src;
        }

        internal override void UpdateDef() {
            manager.UpdateDef(signature.ToString(), ToJObject(true));
        }
    }
}
