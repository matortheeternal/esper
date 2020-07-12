using esper.data;
using esper.elements;
using esper.parsing;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace esper.defs.values {
    public class IntDef<T> : ValueDef {
        private static Dictionary<Type, string> intDefTypes =
            new Dictionary<Type, string> {
                { typeof(byte), "uint8" },
                { typeof(UInt16), "uint16" },
                { typeof(UInt32), "uint32" },
                { typeof(sbyte), "int8" },
                { typeof(Int16), "int16" },
                { typeof(Int32), "int32" },
            };

        public static string defType {
            get => intDefTypes[typeof(T)];
        }

        public int size { get => 4; }

        public IntDef(DefinitionManager manager, JObject src, Def parent = null)
            : base(manager, src, parent) {}

        public DataContainer ReadData(PluginFileSource source) {
            return new IntData<T>(source);
        }

        public new void SetValue(ValueElement element, string value) {
            var info = typeof(T).GetMethod("Parse");
            T data = (T) info.Invoke(null, new object[] { value });
            element.data = new IntData<T>(data);
        }
    }
}
