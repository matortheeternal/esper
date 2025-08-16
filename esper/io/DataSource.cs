using esper.plugins;
using System.Text;

namespace esper.io {
    public class DataSource {
        internal BinaryReader reader;

        internal virtual Stream stream => throw new NotImplementedException();
        internal virtual long dataEndPos => throw new NotImplementedException();
        internal virtual PluginFile plugin => throw new NotImplementedException();

        internal bool localized => plugin.localized;
        internal Encoding stringEncoding => plugin.sessionOptions.encoding;

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

        internal void ReadMultiple(UInt32 dataSize, Action readEntity) {
            var endOffset = stream.Position + dataSize;
            while (stream.Position < endOffset) readEntity();
            if (stream.Position > endOffset)
                throw new Exception("Critical error parsing file, read past end offset.");
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
            byte[] bytes = reader.ReadBytes((int)size);
            return stringEncoding.GetString(bytes);
        }
    }
}
