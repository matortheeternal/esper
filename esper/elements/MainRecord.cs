using esper.data;
using esper.parsing;
using System.IO.MemoryMappedFiles;

namespace esper.elements {
    public class MainRecord : Container {
        public readonly StructElement header;
        public ulong formId {
            get => header.GetData<IntData<ulong>>("Form ID").data;
        }
        public ulong localFormId {
            get => formId & 0xFFFFFF;
        }

        public MainRecord(Container container, Def def) 
            : base(container, def) { }

        public static MainRecord Read(
            MemoryMappedViewStream stream, 
            Container container, 
            Signature signature
        ) {
            var def = container.manager.GetRecordDef(signature);
            var record = new MainRecord(container, def);
            return record;
        }

        public bool IsLocal() {
            // TODO
            return true;
        }
    }
}
