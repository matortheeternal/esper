﻿using esper.elements;
using esper.parsing;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;

namespace esper.defs {
    public class Int0Def : ValueDef {
        public static readonly string defType = "int0";
        public override int? size => 0;

        public Int0Def(DefinitionManager manager, JObject src, Def parent)
            : base(manager, src, parent) {}

        public override dynamic ReadData(PluginFileSource source, UInt16? dataSize) {
            return null;
        }

        public override dynamic DefaultData() {
            return null;
        }

        public override string GetValue(ValueElement element) {
            return "";
        }

        public override void SetValue(ValueElement element, string value) {
            element.data = null;
        }
    }
}