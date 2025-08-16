using esper.plugins;

namespace esper.setup {
    [JSExport]
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

        public virtual void ReplaceWithDummy() {
            plugin = new PluginFile(plugin.session, plugin.filename, new PluginFileOptions { });
            plugin.container = plugin.session.pluginManager.root;
        }
    }
}
