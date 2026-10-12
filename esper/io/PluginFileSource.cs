using esper.data;
using esper.elements;
using esper.plugins;
using System.IO.Compression;
using System.IO.MemoryMappedFiles;

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
            if (dataSize < sizeof(UInt32))
                throw new InvalidDataException("Compressed record body is too small.");

            var decompressedDataSize = reader.ReadUInt32();
            var compressedSize = checked((int)dataSize - sizeof(UInt32));
            var compressedData = reader.ReadBytes(compressedSize);
            if (compressedData.Length != compressedSize)
                throw new EndOfStreamException("Compressed record body was truncated.");

            return Decompress(compressedData, decompressedDataSize);
        }

        internal static byte[] Decompress(
            byte[] compressedData, UInt32 decompressedDataSize
        ) {
            var data = new byte[checked((int)decompressedDataSize)];
            try {
                using var compressedStream = new MemoryStream(compressedData);
                using var zstream = new ZLibStream(
                    compressedStream, CompressionMode.Decompress
                );
                ReadDecompressed(zstream, data);
            } catch (InvalidDataException) when (HasZlibWrapper(compressedData)) {
                // Some official plugins contain valid DEFLATE data with a bad
                // Adler-32 trailer. Preserve compatibility with those records.
                using var compressedStream = new MemoryStream(
                    compressedData, 2, compressedData.Length - 6, false
                );
                using var dstream = new DeflateStream(
                    compressedStream, CompressionMode.Decompress
                );
                ReadDecompressed(dstream, data);
            }
            return data;
        }

        private static void ReadDecompressed(Stream stream, byte[] data) {
            stream.ReadExactly(data);
            if (stream.ReadByte() != -1) {
                throw new InvalidDataException(
                    $"Decompressed record body exceeds {data.Length} bytes."
                );
            }
        }

        private static bool HasZlibWrapper(byte[] data) {
            if (data.Length < 6) return false;
            var cmf = data[0];
            var flg = data[1];
            return (cmf & 0x0F) == 8 && (cmf >> 4) <= 7 &&
                ((cmf << 8) + flg) % 31 == 0 && (flg & 0x20) == 0;
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
