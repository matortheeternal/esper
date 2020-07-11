using esper.elements;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace esper.parsing {
    public class PluginFileSource {
        private readonly Signature GRUP = Signature.FromString("GRUP");

        private readonly MemoryMappedFile file;
        private readonly MemoryMappedViewStream stream;

        public PluginFileSource(string filePath) {
            file = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, filePath);
            stream = file.CreateViewStream();
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
            file.header = MainRecord.Read(stream, file, tes4);
        }

        public void ReadRecords(PluginFile file) {
            while (stream.Position <= stream.Length) {
                unsafe {
                    void* ptr = (void*)stream.PositionPointer;
                    Span<byte> span = new Span<byte>(ptr, 4);
                    var signature = new Signature(span[0], span[1], span[2], span[3]);
                    if (signature == GRUP) {
                        GroupRecord.Read(stream, file);
                    } else {
                        MainRecord.Read(stream, file, signature);
                    }
                }
            }
                
        }
    }
}
