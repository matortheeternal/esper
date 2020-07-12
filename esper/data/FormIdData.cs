using esper.parsing;
using esper.plugins;
using System;

namespace esper.data {
    public class FormIdData : DataContainer {
        public readonly PluginFile targetPlugin;
        public readonly UInt32 formId;

        public FormIdData(PluginFile targetPlugin, UInt32 formId) {
            this.targetPlugin = targetPlugin;
            this.formId = formId;
        }

        public FormIdData(PluginFileSource source) {
            UInt32 data = source.reader.ReadUInt32();
            byte ordinal = (byte) (data >> 24);
            targetPlugin = source.plugin.OrdinalToFile(ordinal);
            formId = data & 0xFFFFFF;
        }
    }
}
