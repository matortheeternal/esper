using esper.data;
using esper.elements;
using esper.plugins;
using System;
using System.IO;

namespace esper.io {
    public class RecordSource : DataSource {
        internal uint dataSize;

        private readonly MainRecord record;
        private Subrecord? _currentSubrecord;
        private long _subrecordEndPos;
        private readonly MemoryStream dataStream;

        internal override PluginFile plugin => record.file;
        internal override long dataEndPos => _subrecordEndPos;
        internal Subrecord currentSubrecord => (Subrecord)_currentSubrecord;
        internal override Stream stream => dataStream;

        public RecordSource(MainRecord record, byte[] data) {
            this.record = record;
            dataSize = (uint) data.Length;
            dataStream = new MemoryStream(data);
            reader = new BinaryReader(stream);
        }

        internal void ReadSubrecord() {
            var subrecord = new Subrecord(this);
            if (subrecord.signature == Signatures.XXXX) {
                var nextSize = reader.ReadUInt32();
                subrecord = new Subrecord(this) {
                    dataSize = nextSize
                };
            }
            _subrecordEndPos = stream.Position + subrecord.dataSize;
            _currentSubrecord = subrecord;
        }

        internal bool NextSubrecord() {
            if (stream.Position == stream.Length) return false;
            if (_currentSubrecord != null) return true;
            ReadSubrecord();
            return true;
        }

        internal void SubrecordHandled() {
            if (stream.Position > _subrecordEndPos)
                throw new Exception("Critical error reading subrecord, read past end offset.");
            if (stream.Position < _subrecordEndPos) {
                // Warn($"Warning: {subrecordEndPos - stream.Position} unread bytes on " +
                //      $"subrecord {currentSubrecord.signature}");
                stream.Position = _subrecordEndPos;
            }
            _currentSubrecord = null;
        }
    }
}
