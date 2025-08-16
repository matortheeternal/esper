using esper.data;
using esper.data.headers;
using esper.elements;
using esper.helpers;
using esper.io;
using esper.plugins;
using esper.setup;

namespace esper.defs {
    using TopGroupsMap = Dictionary<Signature, GroupDef>;

    [JSExport]
    public class PluginFileDef : ElementDef {
        public static string defId = "pluginFile";

        internal List<ElementDef> childrenDefs;
        internal readonly TopGroupsMap topGroups;

        public override List<ElementDef> childDefs => childrenDefs;

        public PluginFileDef(DefinitionManager manager, JObject src)
            : base(manager, src) {
            childrenDefs = JsonHelpers.Defs<ElementDef>(manager, src, "children");
            topGroups = BuildTopGroupsMap();
        }

        private TopGroupsMap BuildTopGroupsMap() {
            var map = new TopGroupsMap(childrenDefs.Count - 1);
            foreach (var def in childrenDefs) {
                if (def is GroupDef groupDef)
                    map[groupDef.signature] = groupDef;
            }
            return map;
        }

        public bool IsTopGroup(Signature sig) {
            return topGroups.ContainsKey(sig);
        }

        internal void ReadFileHeader(PluginFile plugin) {
            var source = plugin.source;
            if (plugin.header != null || source == null) return;
            var magic = source.PeekSignature();
            if (magic != Signatures.TES4)
                throw new Exception($"Expected plugin file to start with TES4, found {magic}");
            plugin.header = MainRecord.Read(plugin, source, Signatures.TES4);
            plugin.session.pluginManager.AddFile(plugin);
            plugin.InitMasters();
            plugin.InitRecordMaps();
        }

        internal void ReadGroups(PluginFile plugin) {
            var source = plugin.source;
            if (source == null) return;
            var endOffset = source.fileSize - 1;
            while (source.stream.Position < endOffset)
                GroupRecord.Read(plugin, source);
            plugin.internalElements.TrimExcess();
            plugin.SortRecords();
        }

        internal override void WriteElement(
            Element element, PluginFileOutput output
        ) {
            output.WriteContainer((Container)element);
        }

        internal override GroupDef ImproviseGroupDef(IGroupHeader header) {
            var groupDef = base.ImproviseGroupDef(header);
            topGroups.Add(new Signature(header.label), groupDef);
            return groupDef;
        }

        internal override GroupDef GetGroupDef(IGroupHeader header) {
            if (header.groupType > 0)
                throw new Exception("Expected top group.");
            var signature = new Signature(header.label);
            if (topGroups.ContainsKey(signature)) return topGroups[signature];
            if (sessionOptions.improvise) return ImproviseGroupDef(header);
            throw new Exception($"Unknown top group {signature}");
        }

        internal override JObject ToJObject(bool isBase = true) {
            var src = base.ToJObject(isBase);
            if (!isBase) return src;
            src.Add("type", defId);
            return src;
        }

        internal override void UpdateDef() {
            manager.UpdateDef("PluginFile", ToJObject());
        }
    }
}
