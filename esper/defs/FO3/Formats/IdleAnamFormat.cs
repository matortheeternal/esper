using esper.elements;
using esper.setup;

namespace esper.defs.FO3 {
    public class IdleAnamFormat : FormatDef {
        public static readonly string defId = "IdleAnamFormat";
        public override bool customSortKey => true;

        internal IdleAnamFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        internal override string DataToValue(ValueElement element, object data) {
            long v = Convert.ToInt64(data);
            string s;
            switch (v & ~0xC0L) {
                case 0: s = "Idle"; break;
                case 1: s = "Movement"; break;
                case 2: s = "Left Arm"; break;
                case 3: s = "Left Hand"; break;
                case 4: s = "Weapon"; break;
                case 5: s = "Weapon Up"; break;
                case 6: s = "Weapon Down"; break;
                case 7: s = "Special Idle"; break;
                case 20: s = "Whole Body"; break;
                case 21: s = "Upper Body"; break;
                default: s = "<Unknown: " + (v & ~0xC0L) + ">"; break;
            }
            if ((v & 0x80L) == 0) s += ", Must return a file";
            if ((v & 0x40L) == 1) s += ", Unknown Flag";
            return s;
        }

        internal override object ValueToData(ValueElement element, string value) {
            throw new NotImplementedException();
        }

        internal override string GetSortKey(ValueElement element, object data) {
            return Convert.ToInt64(data).ToString("X2");
        }
    }
}
