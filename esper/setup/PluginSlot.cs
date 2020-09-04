using esper.plugins;
using System;

namespace esper.setup {
    public class PluginSlot {
        public PluginFile plugin;

        public PluginSlot(PluginFile plugin) {
            this.plugin = plugin;
            plugin.pluginSlot = this;
        }

        public virtual UInt32 GetOrdinal() {
            throw new NotImplementedException();
        }

        public virtual UInt32 FormatFormId(UInt32 localFormId) {
            throw new NotImplementedException();
        }
    }
}
