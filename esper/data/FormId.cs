﻿using esper.elements;
using esper.plugins;
using System;
using System.Collections.Generic;

namespace esper.data {
    public class FormId {
        public static List<FormIdFormat> formats = new List<FormIdFormat> {
            new BraceFormat()
        };

        public PluginFile targetPlugin;
        public UInt32 fileFormId;

        public UInt32 localFormId => fileFormId & 0xFFFFFF;

        public string targetFileName {
            get {
                if (localFormId < 0x800) return "Hardcoded";
                if (targetPlugin != null) return targetPlugin.filename;
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

        public override string ToString() {
            return formats[0].ToString(this);
        }

        public static FormId Parse(ValueElement element, string value) {
            return formats[0].Parse(element, value);
        }
    }
}