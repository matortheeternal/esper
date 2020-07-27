using esper.plugins;
using esper.elements;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;

namespace esper.parsing {
    public class PluginFileSource {
        private readonly Signature GRUP = Signature.FromString("GRUP");
        private readonly Signature TES4 = Signature.FromString("TES4");

        private readonly MemoryMappedFile file;
        private readonly MemoryMappedViewStream fileStream;
        private readonly BinaryReader fileReader;
        private UnmanagedMemoryStream decompressedStream;
        private BinaryReader decompressedReader;

        public readonly PluginFile plugin;
        public string filePath;

        public bool localized => plugin.localized;
        public Encoding stringEncoding { get => plugin.stringEncoding; }
        public UnmanagedMemoryStream stream { 
            get => decompressedStream ?? fileStream;
        }
        public BinaryReader reader {
            get => decompressedReader ?? fileReader;
        }

        public PluginFileSource(string filePath, PluginFile plugin) {
            this.filePath = filePath;
            this.plugin = plugin;
            plugin.source = this;
            file = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open);
            fileStream = file.CreateViewStream();
            fileReader = new BinaryReader(stream);
        }

        public Signature ReadSignature() {
            byte[] bytes = reader.ReadBytes(4);
            return new Signature(bytes[0], bytes[1], bytes[2], bytes[3]);
        }

        public void ReadFileHeader(PluginFile file) {
            file.header = MainRecord.Read(this, file, TES4);
        }

        public LocalizedString ReadLocalizedString() {
            var id = reader.ReadUInt32();
            return new LocalizedString(plugin, id);
        }

        public string ReadString() {
            // TODO: read until null terminator
            return "";
        }

        public string ReadString(int? size) {
            if (size == null) return ReadString();
            byte[] bytes = reader.ReadBytes((int) size);
            return stringEncoding.GetString(bytes);
        }

        public IntPtr GetIntPtr() {
            unsafe { return (IntPtr)stream.PositionPointer; }
        }

        public void ReadRecords(PluginFile file) {
            while (stream.Position <= stream.Length) {
                unsafe {
                    var signature = ReadSignature();
                    if (signature == GRUP) {
                        GroupRecord.Read(this, file);
                    } else {
                        MainRecord.Read(this, file, signature);
                    }
                }
            }
        }

        public void Decompress(UInt32 dataSize) {
            // TODO
        }

        public void DiscardDecompressedStream() {
            decompressedStream = null;
            decompressedReader = null;
        }

        public long GetOffset(UInt32 dataSize) {
            if (decompressedStream != null) 
                return decompressedStream.Length - 1;
            return stream.Position + dataSize;
        }

        public void ReadMultiple(UInt32 dataSize, Action readEntity) {
            var endOffset = GetOffset(dataSize);
            try {
                while (stream.Position < endOffset) readEntity();
            } finally {
                DiscardDecompressedStream();
            }
        }
    }
}
