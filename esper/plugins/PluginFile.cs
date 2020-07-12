using esper.parsing;
using esper.setup;
using esper.elements;
using System.Text;

namespace esper.plugins {
    public class PluginFile : Container {
        public MainRecord header;
        public Session session;
        public string filename;
        public PluginFileOptions options;
        public PluginFileSource source;
        public Encoding stringEncoding { get => session.options.encoding; }

        public string filePath {
            get {
                if (source == null) return null;
                return source.filePath;
            }
        }

        public new DefinitionManager manager {
            get {
                return session.definitionManager;
            }
        }

        public new PluginFile file {
            get {
                return this;
            }
        }

        public PluginFile(Session session, string filename, PluginFileOptions options)
            : base() {
            this.session = session;
            this.filename = filename;
            this.options = options;
        }

        public bool IsEsl() {
            return false; // TODO
        }

        public bool IsDummy() {
            return source == null;
        }

        public void ReadFileHeader() {
            if (header != null) return;
            source.ReadFileHeader(this);
            InitMasters();
            InitMasterIndexes();
        }
    }
}
