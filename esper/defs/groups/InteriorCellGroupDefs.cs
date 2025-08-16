using esper.elements;
using esper.setup;

namespace esper.defs {
    [JSExport]
    public class InteriorCellGroupDef : GroupDef {
        internal virtual Regex nameExpr => throw new NotImplementedException();

        public InteriorCellGroupDef(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        public override dynamic ConvertLabel(GroupRecord group, byte[] label) {
            return BitConverter.ToInt32(label);
        }

        public override bool NameMatches(string name) {
            return nameExpr.IsMatch(name);
        }

        internal override byte[] ParseLabel(Container container, string name) {
            var match = nameExpr.Match(name);
            Int32 label = Int32.Parse(match.Groups[1].Value);
            return BitConverter.GetBytes(label);
        }
    }

    [JSExport]
    public class InteriorCellBlockDef : InteriorCellGroupDef {
        internal override Regex nameExpr => new Regex(@"^Block (\-?\d+)$");

        public static int defGroupType = 2;
        public override int groupType => 2;

        public InteriorCellBlockDef(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        public override string GetName(GroupRecord group) {
            return $"Block {group.label.ToString()}";
        }
    }

    [JSExport]
    public class InteriorCellSubBlockDef : InteriorCellGroupDef {
        internal override Regex nameExpr => new Regex(@"^Sub-Block (\-?\d+)$");

        public static int defGroupType = 3;
        public override int groupType => 3;

        public InteriorCellSubBlockDef(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        public override string GetName(GroupRecord group) {
            return $"Sub-Block {group.label.ToString()}";
        }
    }
}
