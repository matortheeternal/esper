using esper.plugins;
using System;

namespace esper.parsing {
    public class LocalizedString {
        public PluginFile plugin;
        public UInt32 id;

        public LocalizedString(PluginFile plugin, UInt32 id) {
            this.id = id;
            this.plugin = plugin;
        }

        public override string ToString() {
            return plugin.GetString(id);
        }
    }
}
