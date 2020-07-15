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
    }
}
