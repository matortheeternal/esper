using esper.elements;
using System;

namespace esper.setup {
    public class PluginSlot {
        public PluginFile plugin;

        public PluginSlot(PluginFile plugin) {
            this.plugin = plugin;
        }

        public int GetOrdinal() {
            throw new NotImplementedException();
        }
    }
}
