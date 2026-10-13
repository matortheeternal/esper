using esper.elements;
using esper.setup;

namespace esper.defs.TES4 {
    public class IdleAnamFormat : FormatDef {
        public static readonly string defId = "IdleAnamFormat";
        public override bool customSortKey => true;

        internal IdleAnamFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        internal override string DataToValue(ValueElement element, dynamic data) {
            long v = Convert.ToInt64(data);
            string s;
            switch (v & ~0x80L) {
                case 0: s = "Lower Body"; break;
                case 1: s = "Left Arm"; break;
                case 2: s = "Left Hand"; break;
                case 3: s = "Right Arm"; break;
                case 4: s = "Special Idle"; break;
                case 5: s = "Whole Body"; break;
                case 6: s = "Upper Body"; break;
                default: s = "<Unknown: " + (v & ~0x80L) + ">"; break;
            }
            if ((v & 0x80L) == 0) s += ", Must return a file";
            return s;
        }

        internal override dynamic ValueToData(ValueElement element, string value) {
            throw new NotImplementedException();
        }

        internal override string GetSortKey(ValueElement element, dynamic data) {
            return Convert.ToInt64(data).ToString("X2");
        }
    }
}
