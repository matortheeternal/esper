using esper.plugins;

namespace esper.setup {
    public class LightPluginSlot : PluginSlot {
        public static ulong BASE_ORDINAL = 0xFE000000;
        public ushort index;

        public LightPluginSlot(PluginFile plugin, int index)
            : base(plugin) {
            this.index = (ushort) index;
        }

        public new ulong GetOrdinal() {
            return BASE_ORDINAL | (ulong)index << 12;
        }
    }
}
