using esper.data;
using esper.elements;
using esper.plugins;
using esper.setup;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace esper.defs {
    public class MainRecordDef : MembersDef {
        public static string defType = "record";
        public StructDef headerDef;

        public MainRecordDef(DefinitionManager manager, JObject src)
            : base(manager, src) {
            headerDef = manager.BuildMainRecordHeaderDef(src, this);
        }

        private void ReadHeader(MainRecord rec, PluginFileSource source) {
            // TODO: get this offset from definition manager
            source.stream.Position = rec.bodyOffset - 24;
            rec.header.ToStructElement(rec, source);
        }

        internal void HandleSubrecord(
            MainRecord rec, PluginFileSource source, ref int defIndex
        ) {
            var subrecord = source.currentSubrecord;
            var def = GetMemberDef(subrecord.signature, ref defIndex);
            if (def == null) {
                rec.unexpectedSubrecords.Add(subrecord);
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

        private void ReadSubrecords(MainRecord rec, PluginFileSource source) {
            int defIndex = 0;
            source.StartRead(rec.dataSize);
            while (true) {
                if (!source.NextSubrecord()) break;
                HandleSubrecord(rec, source, ref defIndex);
            }
            source.EndRead();
        }

        internal void ReadElements(MainRecord rec, PluginFileSource source) {
            rec._elements = new List<Element>();
            rec.unexpectedSubrecords = new List<Subrecord>();
            ReadHeader(rec, source);
            source.WithRecordData(rec, () => {
                ReadSubrecords(rec, source);
                rec.ElementsReady();
            });
        }

        internal void InitElement(MainRecord rec) {
            foreach (var def in memberDefs) {
                if (!def.required) continue;
                var e = def.NewElement(rec);
                e.Initialize();
            }
        }
    }
}
