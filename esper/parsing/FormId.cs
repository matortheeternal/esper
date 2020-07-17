using esper.plugins;
using System;

namespace esper.parsing {
    public struct FormId {
        public PluginFile targetPlugin;
        public UInt32 localFormId;
        public string targetFileName {
            get => targetPlugin != null ? targetPlugin.filename : "Hardcoded";
        }

        public FormId(PluginFile targetPlugin, UInt32 localFormId) {
            this.targetPlugin = targetPlugin;
            this.localFormId = localFormId;
        }

        public override string ToString() {
            return string.Format(
                "{{{0}:{1}}}",
                targetFileName,
                localFormId.ToString("X6")
            );
        }
    }
}
