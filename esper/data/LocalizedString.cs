﻿using esper.elements;
using esper.plugins;
using System;

namespace esper.data {
    public class LocalizedString {
        public PluginFile plugin;
        public UInt32 id;

        public LocalizedString(PluginFile plugin, UInt32 id) {
            this.id = id;
            this.plugin = plugin;
        }

        public string ToString(Element element) {
            return plugin.GetString(id, element.record);
        }

        internal void WriteTo(PluginFileOutput output) {
            output.writer.Write(id);
        }

        internal static LocalizedString Read(PluginFileSource source) {
            var id = source.reader.ReadUInt32();
            return new LocalizedString(source.plugin, id);
        }
    }
}
