using System.Collections.ObjectModel;

namespace esper.plugins {
    [JSExport]
    public class MasterList {
        private readonly List<PluginFile> _files;

        public int Count => _files.Count;

        public ReadOnlyCollection<PluginFile> files {
            get => _files.AsReadOnly();
        }
        public List<string> filenames {
            get => files.Select(f => f.filename).ToList();
        }
        public PluginFile parentFile;

        public MasterList(PluginFile parentFile, List<PluginFile> files = null) {
            this.parentFile = parentFile;
            _files = files != null 
                ? new List<PluginFile>(files) 
                : new List<PluginFile>();
        }

        public PluginFile OrdinalToFile(byte ordinal) {
            if (ordinal >= files.Count) return parentFile;
            return files[ordinal];
        }

        public byte FileToOrdinal(PluginFile file) {
            int index = files.IndexOf(file);
            return (byte) (index < 0 ? files.Count : index);
        }

        public void Add(PluginFile file) {
            _files.Add(file);
        }

        public void Remove(PluginFile file) {
            _files.RemoveAll(p => p == file);
        }

        public bool Contains(PluginFile file) {
            return _files.Contains(file);
        }
    }

    [JSExport]
    public class ReadOnlyMasterList : MasterList {
        public ReadOnlyMasterList(PluginFile parentFile, List<PluginFile> files)
            : base(parentFile, files) {}

        public new void Add(PluginFile file) {
            throw new Exception("Cannot add files to a ReadOnlyMasterList.");
        }

        public new void Remove(PluginFile file) {
            throw new Exception("Cannot remove files from a ReadOnlyMasterList.");
        }
    }
}
