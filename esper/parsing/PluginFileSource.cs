using esper.plugins;
using esper.elements;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using esper.setup;

namespace esper.parsing {
    public class PluginFileSource {
        private readonly Signature GRUP = Signature.FromString("GRUP");

        private readonly MemoryMappedFile file;
        public readonly MemoryMappedViewStream stream;
        public readonly PluginFile plugin;
        public readonly BinaryReader reader;
        public string filePath;
        public bool localized => plugin.localized;
        public Encoding stringEncoding { get => plugin.stringEncoding; }

        public PluginFileSource(string filePath, PluginFile plugin) {
            this.filePath = filePath;
            this.plugin = plugin;
            plugin.source = this;
            file = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open);
            stream = file.CreateViewStream();
            reader = new BinaryReader(stream);
        }

        public List<Subrecord> ReadSubrecords(long dataSize) {
            var subrecords = new List<Subrecord>();
            long endPos = stream.Position + dataSize;
            while (stream.Position < endPos)
                subrecords.Add(Subrecord.Read(stream));
            return subrecords;
        }

        public void ReadFileHeader(PluginFile file) {
            var tes4 = Signature.FromString("TES4");
            file.header = MainRecord.Read(this, file, tes4);
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
                    void* ptr = (void*)stream.PositionPointer;
                    Span<byte> span = new Span<byte>(ptr, 4);
                    var signature = new Signature(span[0], span[1], span[2], span[3]);
                    if (signature == GRUP) {
                        GroupRecord.Read(this, file);
                    } else {
                        MainRecord.Read(this, file, signature);
                    }
                }
            }
                
        }
    }
}
