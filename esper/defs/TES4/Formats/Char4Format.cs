using esper.elements;
using esper.setup;
using System.Text;

namespace esper.defs.TES4 {
    public class Char4Format : FormatDef {
        public static readonly string defId = "Char4Format";

        internal Char4Format(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        internal override string DataToValue(ValueElement element, ValueData data) {
            var bytes = BitConverter.GetBytes(Convert.ToUInt32(data.ToInt64()));
            return Encoding.ASCII.GetString(bytes);
        }

        internal override ValueData ValueToData(ValueElement element, string value) {
            if (string.IsNullOrEmpty(value)) return (uint)0x20202020;
            if (value.Length != 4)
                throw new Exception("The value must be exactly 4 characters");
            return BitConverter.ToUInt32(Encoding.ASCII.GetBytes(value), 0);
        }
    }
}
