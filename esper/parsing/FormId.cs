using esper.plugins;
using System;

namespace esper.parsing {
    public struct FormId {
        public PluginFile targetPlugin;
        public UInt32 localFormId;
        public string targetFileName {
            get {
                if (localFormId < 0x800) return "Hardcoded";
                if (targetPlugin != null) return targetPlugin.filename;
                return "Error";
            }
        }

        public FormId(PluginFile targetPlugin, UInt32 localFormId) {
            this.targetPlugin = targetPlugin;
            this.localFormId = localFormId;
        }

        public override string ToString() {
            if (localFormId == 0) return "{Null:000000}";
            return string.Format(
                "{{{0}:{1}}}",
                targetFileName,
                localFormId.ToString("X6")
            );
        }
    }
}
