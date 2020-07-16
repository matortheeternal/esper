using esper.data;
using esper.elements;
using esper.helpers;
using esper.parsing;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs {
    public class BytesDef : ValueDef {
        public static string defType = "bytes";

        public BytesDef(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) {
            if (size < 0) throw new Exception("Def source has invalid size" + size);
        }

        public new DataContainer ReadData(PluginFileSource source) {
            var data = source.reader.ReadBytes(size);
            return new BytesData(data);
        }

        public new DataContainer DefaultData() {
            return new BytesData(new byte[size]);
        }

        public new void SetValue(ValueElement element, string value) {
            byte[] data = StringHelpers.ParseBytes(value);
            if (data.Length != size)
                throw new Exception("Bytes size mismatch");
            element.data = new BytesData(data);
        }
    }
}
