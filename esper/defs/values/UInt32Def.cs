﻿using esper.elements;
using esper.parsing;
using esper.setup;
using System;
using Newtonsoft.Json.Linq;
using esper.helpers;

namespace esper.defs {
    public class UInt32Def : ValueDef {
        public static readonly string defType = "uint32";
        public override int? size => 4;

        public UInt32Def(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) { }

        public override dynamic ReadData(PluginFileSource source, UInt16? size) {
            return source.reader.ReadUInt32();
        }

        public override dynamic DefaultData() {
            return 0;
        }

        public override void SetData(ValueElement element, dynamic data) {
            element.data = sessionOptions.clampIntegerValues
                ? DataHelpers.ClampToUInt32(data)
                : (UInt32)data;
            element.SetState(ElementState.Modified);
        }
    }
}