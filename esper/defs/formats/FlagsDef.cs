using esper.elements;
using esper.helpers;
using esper.setup;
using System.Text;

namespace esper.defs {
    public class FlagsDef : FormatDef {
        internal static Regex unknownFlagExpr = new Regex(@"^Unknown (\d+)$");
        public static readonly string defId = "flags";

        //public override bool customSortKey => true;

        public Dictionary<int, string> flags;

        internal FlagsDef(DefinitionManager manager, JObject src)
            : base(manager, src) {
            flags = JsonHelpers.Flags(src, "flags");
        }

        public string GetFlagValue(int index) {
            if (!flags.ContainsKey(index)) return "Unknown " + index;
            return flags[index];
        }

        public int GetFlagIndex(string flag) {
            foreach (var (key, value) in flags)
                if (value == flag) return key;
            Match match = unknownFlagExpr.Match(flag);
            if (!match.Success) return -1;
            return int.Parse(match.Groups[1].Value);
        }

        internal List<string> DataToArray(ValueElement element, VariantData data) {
            var list = new List<string>();
            var numBits = 8 * element.valueDef.size;
            var u64data = (ulong)data.ToInt64();
            for (int i = 0; i < numBits; i++) {
                var flagOrd = (UInt64)1 << i;
                if ((u64data & flagOrd) != 0) list.Add(GetFlagValue(i));
            }
            return list;
        }

        internal override string DataToValue(ValueElement element, VariantData data) {
            return string.Join(", ", DataToArray(element, data));
        }

        internal bool FlagIsSet(VariantData data, string flag) {
            var flagIndex = GetFlagIndex(flag);
            if (flagIndex == -1) return false;
            return FlagIsSet(data, flagIndex);
        }

        internal bool FlagIsSet(VariantData data, int flagIndex) {
            return (((ulong)data.ToInt64() & ((UInt64)1 << flagIndex))) != 0;
        }

        public void SetFlag(ValueElement element, string flag, bool state) {
            var flagIndex = GetFlagIndex(flag);
            if (flagIndex == -1) throw new Exception($"Unknown flag {flag}");
            if (state) {
                element.data = ((ulong)element.data.ToInt64()) | ((UInt64)1 << flagIndex);
            } else {
                element.data = ((ulong)element.data.ToInt64()) & ~((UInt64)1 << flagIndex);
            }
        }

        internal override VariantData ValueToData(ValueElement element, string value) {
            var flags = value.Split(", ");
            UInt64 v = 0;
            foreach (var flag in flags) {
                var flagIndex = GetFlagIndex(flag);
                if (flagIndex == -1) throw new Exception($"Unknown flag {flag}");
                v |= (UInt64)1 << flagIndex;
            }
            return v;
        }

        // I think this only existed because of the flgUnusedMask 
        // and flgDontShows in xEdit, which we may end up not having
        internal override string GetSortKey(ValueElement element, VariantData data) {
            UInt64 v = (ulong)data.ToInt64();
            StringBuilder sortKey = new StringBuilder(new string('0', 64));
            for (int i = 0; i < 64; i++)
                if ((v & ((UInt64)1 << i)) != 0)
                    sortKey[i] = '1';
            return sortKey.ToString();
        }
    }
}
