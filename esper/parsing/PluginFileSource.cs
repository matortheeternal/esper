using esper.plugins;
using esper.elements;
using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;

namespace esper.parsing {
    public class PluginFileSource {
        private readonly Signature TES4 = Signature.FromString("TES4");

        private readonly MemoryMappedFile file;
        private readonly MemoryMappedViewStream fileStream;
        private readonly BinaryReader fileReader;
        private UnmanagedMemoryStream decompressedStream;
        private BinaryReader decompressedReader;
        private readonly FileInfo fileInfo;

        public readonly PluginFile plugin;
        public string filePath;
        public long fileSize => fileInfo.Length;

        internal bool localized => plugin.localized;
        internal Encoding stringEncoding { get => plugin.stringEncoding; }
        internal UnmanagedMemoryStream stream { 
            get => decompressedStream ?? fileStream;
        }
        internal BinaryReader reader {
            get => decompressedReader ?? fileReader;
        }

        public UInt32? ReadPrefix(int? prefix, int? padding) {
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

        internal IntPtr GetIntPtr() {
            unsafe { return (IntPtr)stream.PositionPointer; }
        }

        internal void Decompress(UInt32 dataSize) {
            // TODO
        }

        internal void DiscardDecompressedStream() {
            decompressedStream = null;
            decompressedReader = null;
        }

        internal long GetOffset(UInt32 dataSize) {
            if (decompressedStream != null) 
                return decompressedStream.Length - 1;
            return stream.Position + dataSize;
        }

        internal void ReadMultiple(UInt32 dataSize, Action readEntity) {
            var endOffset = GetOffset(dataSize);
            try {
                while (stream.Position < endOffset) readEntity();
            } finally {
                DiscardDecompressedStream();
            }
        }
    }
}
