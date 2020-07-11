using esper.parsing;
using System.IO.MemoryMappedFiles;

namespace esper.elements {
    public class MainRecord : Container {
        public readonly StructElement header;

        public MainRecord(Container container, Def def) 
            : base(container, def) { }

        public static MainRecord Read(
            MemoryMappedViewStream stream, 
            Container container, 
            Signature signature
        ) {
            var def = container.manager.GetRecordDef(signature);
            var record = new MainRecord(container, );
            return record;
        }
    }
}
