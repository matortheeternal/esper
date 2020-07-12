using esper.data;
using esper.elements;
using esper.parsing;
using esper.setup;
using Newtonsoft.Json.Linq;

namespace esper.defs.values {
    public class Int32Def : ValueDef {
        public static string defType = "int32";

        public int size { get => 4; }

        public Int32Def(DefinitionManager manager, JObject src, Def parent = null)
            : base(manager, src, parent) {}

        public DataContainer ReadData(PluginFileSource source) {
            return new Int32Data(source);
        }

        public new void SetValue(ValueElement element, string value) {
            element.data = new UserInt32Data(value);
        }
    }

    public class Int16Def : ValueDef {
        public static string defType = "int16";

        public int size { get => 2; }

        public Int16Def(DefinitionManager manager, JObject src, Def parent = null)
            : base(manager, src, parent) { }

        public DataContainer ReadData(PluginFileSource source) {
            return new Int16Data(source);
        }

        public new void SetValue(ValueElement element, string value) {
            element.data = new UserInt16Data(value);
        }
    }

    public class Int8Def : ValueDef {
        public static string defType = "int8";

        public int size { get => 1; }

        public Int8Def(DefinitionManager manager, JObject src, Def parent = null)
            : base(manager, src, parent) { }

        public DataContainer ReadData(PluginFileSource source) {
            return new Int8Data(source);
        }

        public new void SetValue(ValueElement element, string value) {
            element.data = new UserInt8Data(value);
        }
    }

    public class UInt32Def : ValueDef {
        public static string defType = "uint32";

        public int size { get => 4; }

        public UInt32Def(DefinitionManager manager, JObject src, Def parent = null)
            : base(manager, src, parent) { }

        public DataContainer ReadData(PluginFileSource source) {
            return new UInt32Data(source);
        }

        public new void SetValue(ValueElement element, string value) {
            element.data = new UserUInt32Data(value);
        }
    }

    public class UInt16Def : ValueDef {
        public static string defType = "uint16";

        public int size { get => 2; }

        public UInt16Def(DefinitionManager manager, JObject src, Def parent = null)
            : base(manager, src, parent) { }

        public DataContainer ReadData(PluginFileSource source) {
            return new UInt16Data(source);
        }

        public new void SetValue(ValueElement element, string value) {
            element.data = new UserUInt16Data(value);
        }
    }

    public class UInt8Def : ValueDef {
        public static string defType = "uint8";

        public int size { get => 1; }

        public UInt8Def(DefinitionManager manager, JObject src, Def parent = null)
            : base(manager, src, parent) { }

        public DataContainer ReadData(PluginFileSource source) {
            return new UInt8Data(source);
        }

        public new void SetValue(ValueElement element, string value) {
            element.data = new UserUInt8Data(value);
        }
    }
}
