﻿using esper.elements;
using esper.helpers;
using esper.io;
using esper.setup;

namespace esper.defs {
    public class BytesDef : ValueDef {
        public static readonly string defId = "bytes";
        public override XEDefType valueDefType => XEDefType.dtByteArray;
        public override SmashType smashType => SmashType.stByteArray;

        public BytesDef(DefinitionManager manager, JObject src)
            : base(manager, src) {
            if (!isVariableSize && fixedSize < 0) 
                throw new Exception("Def source has invalid size" + fixedSize);
        }

        public override string DataToSortKey(dynamic data) {
            return data.ToString();
        }

        public override dynamic ReadData(DataSource source, UInt32? dataSize) {
            if (isVariableSize && dataSize == null) 
                throw new Exception("Cannot read data of unknown size.");
            // return empty array if there are no bytes to read
            if (dataSize == null && fixedSize == 0) return new byte[0];
            // if fixedSize is not null and is not 0, use it
            var numBytes = fixedSize != null && fixedSize > 0 ? fixedSize : (int?) dataSize;
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

        internal override UInt32 GetSize(Element element) {
            UInt32 size = (UInt32) (IsSubrecord() ? 6 : 0);
            var v = (ValueElement)element;
            byte[] bytes = v.data as byte[] ?? DefaultData();
            return (UInt32) (size + bytes.Length);
        }

        internal override void WriteData(
            ValueElement element, PluginFileOutput output
        ) {
            var data = element.data as byte[] ?? DefaultData();
            output.writer.Write(data);
        }

        internal override JObject ToJObject(bool isBase = true) {
            var src = base.ToJObject(isBase);
            if (!isBase) return src;
            src.Add("type", defId);
            return src;
        }
    }
}
