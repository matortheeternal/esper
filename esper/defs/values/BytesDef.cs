﻿using esper.elements;
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
            if (!isVariableSize && size < 0) 
                throw new Exception("Def source has invalid size" + size);
        }

        public override dynamic ReadData(PluginFileSource source, UInt16? dataSize) {
            if (isVariableSize && dataSize == null) 
                throw new Exception("Cannot read data of unknown size.");
            return source.reader.ReadBytes((int) (size ?? dataSize));
        }

        public override dynamic DefaultData() {
            if (isVariableSize) return new byte[0];
            return new byte[(int)size];
        }

        public override void SetData(ValueElement element, dynamic data) {
            if (!isVariableSize && data.Length != size)
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
