using esper.parsing;
using esper.setup;
using System;
using System.Collections.Generic;

namespace esper.elements {
    public struct PluginFileOptions {
        public bool temporary;
        public bool autoload;
    }

    public class PluginFile : Container {
        public MainRecord header;
        public Session session;
        public string filename;
        public PluginFileOptions options;
        public PluginFileSource source;

        public new DefinitionManager manager {
            get {
                return session.definitionManager;
            }
        }

        public PluginFile(Session session, string filename, PluginFileOptions options) {
            this.session = session;
            this.filename = filename;
            this.options = options;
        }

        public bool IsEsl() {
            return false; // TODO
        }

        public void ReadFileHeader() {
            throw new NotImplementedException();
        }

        public List<string> GetMasterFileNames() {
            throw new NotImplementedException();
        }
    }
}
