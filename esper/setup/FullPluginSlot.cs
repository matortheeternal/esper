using esper.elements;

namespace esper.setup {
    public class FullPluginSlot : PluginSlot {
        public byte index;

        public FullPluginSlot(PluginFile plugin, int index)
            : base(plugin) {
            this.index = (byte)index;
        }

        public new ulong GetOrdinal() {
            return (ulong)index << 24;
        }
    }
}
