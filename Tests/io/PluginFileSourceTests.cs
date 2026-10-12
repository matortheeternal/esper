using esper.io;
using NUnit.Framework;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Tests.io {
    public class PluginFileSourceTests {
        private static readonly byte[] Data = Encoding.ASCII.GetBytes(
            "A compressed record body used to verify zlib handling."
        );

        private static byte[] Compress(byte[] data) {
            using var output = new MemoryStream();
            using (var zlib = new ZLibStream(
                output, CompressionLevel.SmallestSize, true
            )) {
                zlib.Write(data);
            }
            return output.ToArray();
        }

        [Test]
        public void DecompressReadsValidZlibData() {
            var compressed = Compress(Data);

            var actual = PluginFileSource.Decompress(compressed, (uint)Data.Length);

            CollectionAssert.AreEqual(Data, actual);
        }

        [Test]
        public void DecompressToleratesInvalidAdler32() {
            var compressed = Compress(Data);
            compressed[^1] ^= 0xFF;

            var actual = PluginFileSource.Decompress(compressed, (uint)Data.Length);

            CollectionAssert.AreEqual(Data, actual);
        }

        [Test]
        public void DecompressRejectsIncorrectDeclaredSize() {
            var compressed = Compress(Data);

            Assert.Throws<InvalidDataException>(() =>
                PluginFileSource.Decompress(compressed, (uint)Data.Length - 1)
            );
            Assert.Throws<EndOfStreamException>(() =>
                PluginFileSource.Decompress(compressed, (uint)Data.Length + 1)
            );
        }
    }
}
