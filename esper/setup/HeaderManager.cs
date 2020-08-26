using esper.data.headers;
using esper.elements;
using esper.plugins;
using System;

namespace esper.setup {
    public class HeaderManager {
        internal Type recordHeaderType;
        internal Type groupHeaderType;
        internal UInt16 recordHeaderSize;
        internal UInt16 groupHeaderSize;

        public HeaderManager(string headerTypeKey) {
            LoadStructTypes(headerTypeKey);
            InitSizes();
        }

        private void LoadStructTypes(string headerTypeKey) {
            var prefix = $"esper.data.headers.{headerTypeKey}";
            recordHeaderType = Type.GetType($"{prefix}RecordHeader");
            groupHeaderType = Type.GetType($"{prefix}GroupHeader");
            if (recordHeaderType == null)
                throw new Exception("Failed to resolve record header struct");
            if (groupHeaderType == null)
                throw new Exception("Failed to resolve group header struct");
        }

        private void InitSizes() {
            var sizeProp = recordHeaderType.GetProperty("size");
            recordHeaderSize = (UInt16)sizeProp.GetValue(null);
            sizeProp = groupHeaderType.GetProperty("size");
            groupHeaderSize = (UInt16)sizeProp.GetValue(null);
        }

        internal dynamic ReadRecordHeader(PluginFileSource source) {
            var args = new object[1] { source };
            return Activator.CreateInstance(recordHeaderType, args);
        }

        internal dynamic ReadGroupHeader(PluginFileSource source) {
            var args = new object[1] { source };
            return Activator.CreateInstance(groupHeaderType, args);
        }

        internal void WriteUpdatedSize(MainRecord rec, PluginFileOutput output) {
            var pos = output.stream.Position;
            UInt32 newSize = (UInt32)(pos - rec.bodyOffset);
            if (newSize == rec.header.dataSize) return;
            var headerSize = output.plugin.manager.headerManager.recordHeaderSize;
            output.stream.Position = rec.bodyOffset - headerSize + 4;
            output.writer.Write(newSize);
            output.stream.Position = pos;
        }

        internal void WriteUpdatedSize(
            GroupRecord group, PluginFileOutput output, long offset
        ) {
            var pos = output.stream.Position;
            UInt32 newSize = (UInt32)(pos - offset);
            if (newSize == group.groupSize) return;
            output.stream.Position = offset - 20;
            output.writer.Write(newSize);
            output.stream.Position = pos;
        }

        internal StructElement HeaderToStructElement(
            MainRecord rec, PluginFileSource source
        ) {
            var method = recordHeaderType.GetMethod("ToStructElement");
            var args = new object[] { rec, source };
            return (StructElement) method.Invoke(rec.header, args);
        }

        internal long WriteGroupHeaderTo(
            IGroupHeader header, PluginFileOutput output
        ) {
            var method = groupHeaderType.GetMethod("WriteTo");
            var args = new object[] { output };
            return (long)method.Invoke(header, args);
        }
    }
}
