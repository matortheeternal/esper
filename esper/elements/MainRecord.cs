﻿using esper.defs;
using esper.parsing;
using esper.resolution;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace esper.elements {
    public class MainRecord : Container, IMainRecord {
        public readonly StructElement header;
        private readonly long bodyOffset;
        public List<Subrecord> unexpectedSubrecords;
        private byte[] decompressedData;

        public MainRecordDef mrDef => def as MainRecordDef;
        public override MainRecord record => this;

        public UInt32 formId => header.GetData("Form ID");
        public UInt32 localFormId => formId & 0xFFFFFF;
        public UInt32 dataSize => header.GetData("Data Size");
        public bool compressed => header.GetFlag("Record Flags", "Compressed");

        public string editorId => this.GetValue("EDID");

        public MainRecord(Container container, ElementDef def) 
            : base(container, def) {}

        public MainRecord(Container container, ElementDef def, PluginFileSource source)
            : base(container, def) {
            header = (StructElement) mrDef.headerDef.ReadElement(this, source);
            bodyOffset = source.stream.Position;
            unexpectedSubrecords = new List<Subrecord>();
            if (sessionOptions.readAllSubrecords) ReadElements(source);
            source.stream.Seek(bodyOffset + dataSize, SeekOrigin.Begin);
        }

        public static MainRecord Read(
            Container container,
            PluginFileSource source,
            Signature signature
        ) {
            var def = (ElementDef) container.manager.GetRecordDef(signature);
            var record = new MainRecord(container, def, source);
            return record;
        }

        private void UnexpectedSubrecord(
            string sig, UInt16 size, PluginFileSource source
        ) {
            var subrecord = new Subrecord(sig, size, source);
            unexpectedSubrecords.Add(subrecord);
        }

        private void ReadSubrecord(PluginFileSource source) {
            var sig = source.ReadSignature().ToString();
            var size = source.reader.ReadUInt16();
            var endPos = source.stream.Position + size;
            var def = mrDef.GetMemberDef(sig.ToString());
            if (def == null) {
                UnexpectedSubrecord(sig, size, source);
            } else if (def.IsSubrecord()) {
                def.ReadElement(this, source, size);
            } else {
                var container = (Container)def.PrepareElement(this);
                def.SubrecordFound(container, source, sig, size);
            }
            source.stream.Position = endPos;
        }

        private bool Decompress(PluginFileSource source) {
            if (decompressedData == null)
                decompressedData = source.Decompress(dataSize);
            source.SetDecompressedStream(decompressedData);
            if (decompressedData != null) return true;
            // Warn("Failed to decompress content");
            return false;
        }

        public void ReadElements(PluginFileSource source) {
            source.stream.Seek(bodyOffset, SeekOrigin.Begin);
            if (compressed && !Decompress(source)) return;
            try {
                source.ReadMultiple(dataSize, () => ReadSubrecord(source));
            } finally {
                source.DiscardDecompressedStream();
            }
        }

        public bool IsLocal() {
            // TODO
            return true;
        }

        public override bool SupportsSignature(string sig) {
            return mrDef.memberDefs.Any(d => d.HasSignature(sig));
        }
    }
}
