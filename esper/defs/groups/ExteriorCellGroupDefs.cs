using esper.elements;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace esper.defs {
    public class ExteriorCellGroupDef : GroupDef {
        internal virtual Regex nameExpr => throw new NotImplementedException();
        public override bool hasRecordParent => true;
        public override bool isChildGroupChild => true;

        public ExteriorCellGroupDef(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        public override dynamic ConvertLabel(GroupRecord group, byte[] label) {
            return new Int16[2] {
                BitConverter.ToInt16(label),
                BitConverter.ToInt16(label, 2)
            };
        }

        public string GetCoordinatesStr(GroupRecord group) {
            return string.Join(", ", group.label);
        }

        public override bool NameMatches(string name) {
            return nameExpr.IsMatch(name);
        }

        internal override byte[] ParseLabel(Container container, string name) {
            var match = nameExpr.Match(name);
            Int16[] label = new Int16[2] {
                Int16.Parse(match.Groups[1].Value),
                Int16.Parse(match.Groups[2].Value)
            };
            return BitConverter.GetBytes(label[0])
                .Concat(BitConverter.GetBytes(label[1]))
                .ToArray();
        }
    }

    public class ExteriorCellBlockDef : ExteriorCellGroupDef {
        internal override Regex nameExpr => new Regex(@"^Block (\-?\d+), (\-\d+)$");

        public static int defGroupType = 4;
        public override int groupType => 4;

        public ExteriorCellBlockDef(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        public override string GetName(GroupRecord group) {
            return $"Block {GetCoordinatesStr(group)}";
        }
    }

    public class ExteriorCellSubBlockDef : ExteriorCellGroupDef {
        internal override Regex nameExpr => new Regex(@"^Sub-Block (\-?\d+), (\-\d+)$");

        public static int defGroupType = 5;
        public override int groupType => 5;

        public ExteriorCellSubBlockDef(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        public override string GetName(GroupRecord group) {
            return $"Sub-Block {GetCoordinatesStr(group)}";
        }
    }
}
