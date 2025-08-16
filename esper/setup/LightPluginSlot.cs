using esper.plugins;

namespace esper.setup {
    [JSExport]
    public class LightPluginSlot : PluginSlot {
        public static UInt32 BASE_ORDINAL = 0xFE000000;
        public UInt16 index;

        public LightPluginSlot(PluginFile plugin, int index)
            : base(plugin) {
            this.index = (UInt16) index;
        }

        public override UInt32 GetOrdinal() {
            return BASE_ORDINAL | (UInt32)index << 12;
        }

        public override UInt32 FormatFormId(UInt32 localFormId) {
            if ((localFormId & 0xFFFFF000) > 0)
                throw new Exception("FormId uses reserved ordinal space.");
            return GetOrdinal() + localFormId;
        }
    }
}
