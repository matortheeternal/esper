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

        public static FormId FromSource(PluginFile sourcePlugin, UInt32 fileFormId) {
            byte ordinal = (byte)(fileFormId >> 24);
            var targetPlugin = sourcePlugin.OrdinalToFile(ordinal, false);
            var formId = fileFormId & 0xFFFFFF;
            return new FormId(targetPlugin, formId);
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
