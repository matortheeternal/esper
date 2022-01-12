using esper.data;
using esper.elements;
using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using Ionic.Zlib;

namespace esper.plugins {
    public class PluginFileSource {
        internal readonly PluginFile plugin;
        internal readonly string filePath;

        private readonly MemoryMappedFile file;
        private readonly MemoryMappedViewStream fileStream;
        private readonly BinaryReader fileReader;
        private Stream decompressedStream;
        private BinaryReader decompressedReader;
        private UInt32 decompressedDataSize;
        private readonly FileInfo fileInfo;
        private Subrecord? _currentSubrecord;
        private long subrecordEndPos;
        private long endDataOffset;

        internal Subrecord currentSubrecord => (Subrecord) _currentSubrecord;
        internal long fileSize => fileInfo.Length;
        internal bool usingDecompressedStream => decompressedStream != null;
        internal bool localized => plugin.localized;
        internal Encoding stringEncoding => plugin.sessionOptions.encoding;
        internal Stream stream => decompressedStream ?? fileStream;
        internal BinaryReader reader => decompressedReader ?? fileReader;

        internal PluginFileSource(string filePath, PluginFile plugin) {
            this.filePath = filePath;
            this.plugin = plugin;
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
            fileReader = new BinaryReader(stream);
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

        internal string ReadString() {
            byte[] bytes = new byte[32];
            int i = 0;
            do {
                byte b = reader.ReadByte();
                if (b == 0) break;
                if (i >= bytes.Length) {
                    var newBytes = new byte[bytes.Length * 2];
                    bytes.CopyTo(newBytes, 0);
                    bytes = newBytes;
                }
                bytes[i++] = b;
            } while (true);
            if (i == 0) return string.Empty;
            return stringEncoding.GetString(bytes, 0, i);
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

        internal void StartRead(UInt32 dataSize) {
            endDataOffset = stream.Position + dataSize;
        }

        internal void EndRead() {
            if (stream.Position > endDataOffset)
                throw new Exception("Critical error parsing file, read past end offset.");
        }

        internal void PipeTo(BinaryWriter writer, int size) {
            // TODO: chunks?
            writer.Write(reader.ReadBytes(size));
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

        internal void ReadSubrecord() {
            var subrecord = new Subrecord(this);
            if (subrecord.signature == "XXXX") {
                var nextSize = reader.ReadUInt32();
                subrecord = new Subrecord(this) {
                    dataSize = nextSize
                };
            }
            subrecordEndPos = stream.Position + subrecord.dataSize;
            _currentSubrecord = subrecord;
        }

        internal bool NextSubrecord() {
            if (stream.Position >= endDataOffset) return false;
            if (_currentSubrecord != null) return true;
            ReadSubrecord();
            return true;
        }

        internal Signature PeekSignature() {
            var pos = stream.Position;
            var sig = Signature.Read(this);
            stream.Position = pos;
            return sig;
        }

        internal void SubrecordHandled() {
            if (stream.Position > subrecordEndPos)
                throw new Exception("Critical error reading subrecord, read past end offset.");
            if (stream.Position < subrecordEndPos) {
                // Warn($"Warning: {subrecordEndPos - stream.Position} unread bytes on " +
                //      $"subrecord {currentSubrecord.signature}");
                stream.Position = subrecordEndPos;
            }
            _currentSubrecord = null;
        }

        internal void WithRecordData(MainRecord rec, Action callback) {
            if (rec.compressed && !rec.Decompress(this))
                throw new Exception("Failed to decompress content.");
            try {
                callback();
            } finally {
                DiscardDecompressedStream();
            }
        }
    }
}
