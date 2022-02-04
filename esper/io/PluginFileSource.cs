using esper.data;
using esper.elements;
using esper.plugins;
using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using Ionic.Zlib;

namespace esper.io {
    public class PluginFileSource : DataSource {
        internal readonly PluginFile _plugin;
        internal readonly string filePath;

        private readonly MemoryMappedFile file;
        private readonly MemoryMappedViewStream fileStream;
        private readonly FileInfo fileInfo;

        internal override PluginFile plugin => _plugin;
        internal override long dataEndPos => fileStream.Length - 1;
        internal long fileSize => fileInfo.Length;
        internal override Stream stream => fileStream;

        internal PluginFileSource(string filePath, PluginFile plugin) {
            this.filePath = filePath;
            _plugin = plugin;
            fileInfo = new FileInfo(filePath);
            plugin.source = this;
            var baseStream = new FileStream(
                filePath, FileMode.Open, 
                FileAccess.Read, FileShare.ReadWrite
            );
            file = MemoryMappedFile.CreateFromFile(
                baseStream, null, 0, MemoryMappedFileAccess.Read, 
                HandleInheritability.None, false
            );
            fileStream = file.CreateViewStream(
                0, 0, MemoryMappedFileAccess.Read
            );
            reader = new BinaryReader(stream);
        }

        internal byte[] Decompress(UInt32 dataSize) {
            var decompressedDataSize = reader.ReadUInt32();
            var zstream = new ZlibStream(fileStream, CompressionMode.Decompress);
            var zreader = new BinaryReader(zstream);
            return zreader.ReadBytes((int) decompressedDataSize);
        }

        internal void PipeTo(BinaryWriter writer, int size) {
            writer.Write(reader.ReadBytes(size));
        }

        internal Signature PeekSignature() {
            var pos = stream.Position;
            var sig = Signature.Read(this);
            stream.Position = pos;
            return sig;
        }

        internal void WithRecordData(MainRecord rec, Action callback) {
            var data = rec.compressed
                ? Decompress(rec.header.dataSize)
                : reader.ReadBytes((int)rec.header.dataSize);
            if (data == null)
                throw new Exception("Failed to read record body.");
            rec.recordSource = new RecordSource(rec, data);
            try {
                callback();
            } finally {
                rec.recordSource = null;
            }
        }
    }
}
