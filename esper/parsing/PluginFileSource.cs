using esper.plugins;
using esper.elements;
using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using Ionic.Zlib;

namespace esper.parsing {
    public class PluginFileSource {
        private readonly Signature TES4 = Signature.FromString("TES4");

        private readonly MemoryMappedFile file;
        private readonly MemoryMappedViewStream fileStream;
        private readonly BinaryReader fileReader;
        private Stream decompressedStream;
        private BinaryReader decompressedReader;
        private UInt32 decompressedDataSize;
        private readonly FileInfo fileInfo;

        public readonly PluginFile plugin;
        public string filePath;
        public long fileSize => fileInfo.Length;

        public bool usingDecompressedStream => decompressedStream != null;
        internal bool localized => plugin.localized;
        internal Encoding stringEncoding { get => plugin.stringEncoding; }
        internal Stream stream { 
            get => decompressedStream ?? fileStream;
        }
        internal BinaryReader reader {
            get => decompressedReader ?? fileReader;
        }

        internal UInt32? ReadPrefix(int? prefix, int? padding) {
            UInt32? size = prefix switch {
                1 => reader.ReadByte(),
                2 => reader.ReadUInt16(),
                4 => reader.ReadUInt32(),
                _ => null
            };
            if (padding != null && size != null)
                reader.ReadBytes((int)padding);
            return size;
        }

        internal PluginFileSource(string filePath, PluginFile plugin) {
            this.filePath = filePath;
            this.plugin = plugin;
            fileInfo = new FileInfo(filePath);
            plugin.source = this;
            file = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open);
            fileStream = file.CreateViewStream();
            fileReader = new BinaryReader(stream);
        }

        internal Signature ReadSignature() {
            byte[] bytes = reader.ReadBytes(4);
            return new Signature(bytes[0], bytes[1], bytes[2], bytes[3]);
        }

        internal void ReadFileHeader(PluginFile file) {
            file.header = MainRecord.Read(file, this, TES4);
            file.header.ReadElements(this);
        }

        internal LocalizedString ReadLocalizedString() {
            var id = reader.ReadUInt32();
            return new LocalizedString(plugin, id);
        }

        internal string ReadString() {
            // TODO: read until null terminator
            return "";
        }

        internal string ReadString(int? size) {
            if (size == null) return ReadString();
            byte[] bytes = reader.ReadBytes((int) size);
            return stringEncoding.GetString(bytes);
        }

        internal byte[] Decompress(UInt32 dataSize) {
            decompressedDataSize = reader.ReadUInt32();
            var zstream = new ZlibStream(fileStream, CompressionMode.Decompress);
            var zreader = new BinaryReader(zstream);
            return zreader.ReadBytes((int) decompressedDataSize);
        }

        internal void ReadMultiple(UInt32 dataSize, Action readEntity) {
            var endOffset = stream.Position + dataSize;
            while (stream.Position < endOffset) readEntity();
            if (stream.Position > endOffset)
                throw new Exception("Critical error parsing file, read past end offset.");
        }

        internal void SetDecompressedStream(byte[] decompressedData) {
            if (decompressedData == null) return;
            decompressedStream = new MemoryStream(decompressedData);
            decompressedReader = new BinaryReader(decompressedStream);
        }

        internal void DiscardDecompressedStream() {
            decompressedStream = null;
            decompressedReader = null;
        }
    }
}
