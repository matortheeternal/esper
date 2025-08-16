using esper.defs;

namespace esper.setup {
    using ClassMap = Dictionary<int, Type>;

    [JSExport]
    public class GroupManager {
        private readonly DefinitionManager manager;
        private readonly ClassMap groupDefClasses = new ClassMap();

        public GroupManager(DefinitionManager manager) {
            this.manager = manager;
        }

        internal Type ResolveDefClass(int groupType) {
            if (!groupDefClasses.ContainsKey(groupType))
                throw new Exception($"Unknown group type: {groupType}");
            return groupDefClasses[groupType];
        }

        internal Def BuildGroupDef(JObject src) {
            int groupType = src.Value<int?>("groupType") ?? 0;
            var defClass = ResolveDefClass(groupType);
            var args = new object[2] { manager, src };
            return (Def)Activator.CreateInstance(defClass, args);
        }

        internal void LoadGroupDefClass(Type type) {
            var info = type.GetField("defGroupType");
            if (info == null) return;
            int key = (int)info.GetValue(null);
            groupDefClasses[key] = type;
        }
    }
}
