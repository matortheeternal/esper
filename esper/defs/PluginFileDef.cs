using esper.data;
using esper.data.headers;
using esper.elements;
using esper.helpers;
using esper.plugins;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace esper.defs {
    using TopGroupsMap = Dictionary<Signature, GroupDef>;

    public class PluginFileDef : ElementDef {
        public static string defId = "pluginFile";

        internal ReadOnlyCollection<ElementDef> childrenDefs;
        internal readonly TopGroupsMap topGroups;

        public override ReadOnlyCollection<ElementDef> childDefs => childrenDefs;

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
            if (plugin.header != null) return;
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

        internal override GroupDef GetGroupDef(IGroupHeader header) {
            if (header.groupType > 0)
                throw new Exception("Expected top group.");
            var signature = new Signature(header.label);
            if (!topGroups.ContainsKey(signature))
                throw new Exception($"Unknown top group {signature}");
            return topGroups[signature];
        }
    }
}
