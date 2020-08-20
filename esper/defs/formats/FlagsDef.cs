using esper.elements;
using esper.helpers;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace esper.defs {
    public class FlagsDef : FormatDef {
        public static Regex unknownFlagExpr = new Regex(@"^Unknown (\d+)$");
        public static string defType = "flags";

        //public override bool customSortKey => true;

        public Dictionary<int, string> flags;

        public FlagsDef(DefinitionManager manager, JObject src)
            : base(manager, src) {
            flags = JsonHelpers.Flags(src, "flags");
        }

        public string GetFlagValue(int index) {
            if (!flags.ContainsKey(index)) return "Unknown " + index;
            return flags[index];
        }

        public int GetFlagIndex(string flag) {
            foreach (var (key, value) in flags)
                if (value == flag) return (int)key;
            Match match = unknownFlagExpr.Match(flag);
            if (!match.Success) return -1;
            return int.Parse(match.Captures[0].Value);
        }

        public List<string> DataToArray(ValueElement element, dynamic data) {
            var list = new List<string>();
            var numBits = 8 * element.valueDef.size;
            for (int i = 0; i < numBits; i++)
                if ((data & (1 << i)) != 0) list.Add(GetFlagValue(i));
            return list;
        }

        public override string DataToValue(ValueElement element, dynamic data) {
            return string.Join(", ", DataToArray(element, data));
        }

        public bool FlagIsSet(dynamic data, string flag) {
            var flagIndex = GetFlagIndex(flag);
            if (flagIndex == -1) return false;
            return (data & (1 << flagIndex)) != 0;
        }

        public override dynamic ValueToData(ValueElement element, string value) {
            // TODO
            throw new NotImplementedException();
        }

        // I think this only existed because of the flgUnusedMask 
        // and flgDontShows in xEdit, which we may end up not having
        public override string GetSortKey(ValueElement element, dynamic data) {
            Int64 v = data;
            StringBuilder sortKey = new StringBuilder(new string('0', 64));
            for (int i = 0; i < 64; i++)
                if ((v & ((Int64)1 << i)) != 0)
                    sortKey[i] = '1';
            return sortKey.ToString();
        }
    }
}
