using esper.parsing;
using esper.plugins;

namespace esper.elements {
    public class GroupRecord : Container {
        public readonly StructElement header;

        public GroupRecord(Container container, PluginFileSource source)
            : base(container) {
            //header = groupRecordHeaderDef.ReadElement(this, source);
           // header.SetState(ElementState.Protected);
        }

        public static GroupRecord Read(PluginFileSource source, PluginFile file) {
            var group = new GroupRecord(file, source);
            //group.ReadRecords();
            return group;
        }
    }
}