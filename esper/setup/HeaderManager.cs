using esper.data.headers;
using esper.elements;
using esper.plugins;
using System;

namespace esper.setup {
    public class HeaderManager {
        internal Type recordHeaderType;
        internal Type groupHeaderType;
        internal UInt32 recordHeaderSize;
        internal UInt32 groupHeaderSize;

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
            recordHeaderSize = (UInt32)sizeProp.GetValue(null);
            sizeProp = groupHeaderType.GetProperty("size");
            groupHeaderSize = (UInt32)sizeProp.GetValue(null);
        }

        internal dynamic ReadRecordHeader(PluginFileSource source) {
            var args = new object[1] { source };
            return Activator.CreateInstance(recordHeaderType, args);
        }

        internal dynamic ReadGroupHeader(PluginFileSource source) {
            var args = new object[1] { source };
            return Activator.CreateInstance(groupHeaderType, args);
        }

        internal StructElement HeaderToStructElement(
            MainRecord rec, PluginFileSource source
        ) {
            var method = recordHeaderType.GetMethod("ToStructElement");
            var args = new object[] { rec, source };
            return (StructElement) method.Invoke(rec.header, args);
        }

        internal void WriteGroupHeaderTo(
            IGroupHeader header, PluginFileOutput output
        ) {
            var method = groupHeaderType.GetMethod("WriteTo");
            var args = new object[] { output };
            method.Invoke(header, args);
        }
    }
}
