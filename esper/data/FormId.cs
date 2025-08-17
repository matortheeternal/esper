﻿using esper.elements;
using esper.plugins;
using esper.io;

namespace esper.data {
    public class FormId {
        public static List<FormIdFormat> formats = new List<FormIdFormat> {
            new BraceFormat()
        };

        public PluginFile targetPlugin;
        public UInt32 fileFormId;

        public UInt32 localFormId => fileFormId & 0xFFFFFF;
        public UInt32? globalFormId {
            get => targetPlugin.pluginSlot?.FormatFormId(localFormId);
        }

        public string targetFileName {
            get {
                if (targetPlugin != null) return targetPlugin.filename;
                if (localFormId < 0x800) return "Hardcoded";
                return "Error";
            }
        }

        public FormId(PluginFile targetPlugin, UInt32 fileFormId) {
            this.targetPlugin = targetPlugin;
            this.fileFormId = fileFormId;
        }

        public static FormId FromSource(PluginFile sourcePlugin, UInt32 fileFormId) {
            byte ordinal = (byte)(fileFormId >> 24);
            var targetPlugin = sourcePlugin.OrdinalToFile(ordinal, false);
            return new FormId(targetPlugin, fileFormId);
        }

        public MainRecord ResolveRecord() {
            if (targetPlugin == null) return null;
            return targetPlugin.GetRecordByFormId(fileFormId);
        }

        public override string ToString() {
            return formats[0].ToString(this);
        }

        public static FormId Parse(ValueElement element, string value) {
            return formats[0].Parse(element, value);
        }

        internal void WriteTo(PluginFileOutput output) {
            byte ordinal = targetPlugin != null
                ? output.plugin.FileToOrdinal(targetPlugin, true)
                : (byte) 0;
            fileFormId = ((UInt32) ordinal << 24) + localFormId;
            output.writer.Write(fileFormId);
        }
    }
}
