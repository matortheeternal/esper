using esper.elements;
using esper.helpers;
using esper.plugins;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs {
    public class BytesDef : ValueDef {
        public static string defType = "bytes";

        public BytesDef(DefinitionManager manager, JObject src)
            : base(manager, src) {
            if (!isVariableSize && fixedSize < 0) 
                throw new Exception("Def source has invalid size" + fixedSize);
        }

        public override dynamic ReadData(PluginFileSource source, UInt16? dataSize) {
            if (isVariableSize && dataSize == null) 
                throw new Exception("Cannot read data of unknown size.");
            // if fixedSize is not null and is not 0, use it
            var numBytes = fixedSize != null && fixedSize > 0 ? fixedSize : dataSize;
            return source.reader.ReadBytes((int) numBytes);
        }

        public override dynamic DefaultData() {
            if (isVariableSize) return new byte[0];
            return new byte[(int)fixedSize];
        }

        public override void SetData(ValueElement element, dynamic data) {
            if (!isVariableSize && data.Length != fixedSize)
                throw new Exception("Bytes size mismatch");
            element._data = data;
        }

        public override string GetValue(ValueElement element) {
            byte[] data = element.data;
            return StringHelpers.FormatBytes(data);
        }

        public override void SetValue(ValueElement element, string value) {
            SetData(element, StringHelpers.ParseBytes(value));
        }
    }
}
