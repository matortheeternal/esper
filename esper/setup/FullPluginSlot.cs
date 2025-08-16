using esper.plugins;

namespace esper.setup {
    [JSExport]
    public class FullPluginSlot : PluginSlot {
        public byte index;

        public FullPluginSlot(PluginFile plugin, int index)
            : base(plugin) {
            this.index = (byte)index;
        }

        public override UInt32 GetOrdinal() {
            return (UInt32)index << 24;
        }

        public override UInt32 FormatFormId(UInt32 localFormId) {
            if ((localFormId & 0xFF000000) > 0)
                throw new Exception("FormId uses reserved ordinal space.");
            return GetOrdinal() + localFormId;
        }
    }
}
