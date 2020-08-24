using esper.data;
using System;
using System.IO;

namespace esper.plugins {
    public class PluginFileOutput {
        private readonly FileStream fileStream;
        private readonly BinaryWriter fileWriter;
        private readonly Stream compressedStream;
        private readonly BinaryWriter compressedWriter;

        internal readonly PluginFile plugin;
        internal readonly string filePath;

        internal Stream stream => compressedStream ?? fileStream;
        internal BinaryWriter writer => compressedWriter ?? fileWriter;

        internal PluginFileOutput(string filePath, PluginFile plugin) {
            this.filePath = filePath;
            this.plugin = plugin;
            plugin.output = this;
            fileStream = File.OpenWrite(filePath);
            fileWriter = new BinaryWriter(fileStream);
        }

        internal void WriteSignature(Signature sig) {
            fileWriter.Write(sig.bytes);
        }

        internal void WriteString(string s) {
            throw new NotImplementedException();
        }
    }
}
