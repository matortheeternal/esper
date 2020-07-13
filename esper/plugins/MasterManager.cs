using esper.elements;
using esper.setup;
using System.Collections.Generic;
using System.Linq;

namespace esper.plugins {
    public interface IMasterManager {
        public PluginFile file { get; }
        internal List<PluginFile> originalMasters { get; }
        public List<PluginFile> masters { get; }
        internal Dictionary<string, byte> originalMasterIndices { get; }
        internal Dictionary<string, byte> masterIndices { get; }
        public List<string> masterFileNames {
            get => masters.Select(m => m.filename).ToList();
        }
    }

    public static class MasterManagerExtensions {
        public static Element GetMastersElement(this IMasterManager m) {
            if (m.file.header == null) return null;
            return m.file.header.GetElement("Master Files");
        }

        public static void InitMasters(this IMasterManager m) {
            Element masterFilesElement = m.GetMastersElement();
            if (masterFilesElement == null) return;
            PluginManager manager = m.file.session.pluginManager;
            List<Element> masterElements = masterFilesElement.GetElements();
            foreach (var element in masterElements) {
                string filename = element.GetValue("MAST");
                m.originalMasters.Add(manager.GetFileByName(filename, true));
            }
            m.masters.AddRange(m.masters);
        }

        public static void InitMasterIndexes(this IMasterManager m) {
            byte index = 0;
            foreach (var master in m.originalMasters)
                m.originalMasterIndices[master.filename] = index++;
            foreach (var entry in m.originalMasterIndices)
                m.masterIndices.Add(entry.Key, entry.Value);
        }

        public static void UpdateMastersElement(this IMasterManager m) {
            Element masterFilesElement = m.GetMastersElement();
            if (masterFilesElement == null) return;
            // TODO
        }

        public static PluginFile OrdinalToFile(this IMasterManager m, byte ordinal) {
            if (ordinal >= m.originalMasters.Count) return m.file;
            return m.originalMasters[ordinal];
        }

        public static byte FileToOrdinal(this IMasterManager m, PluginFile file) {
            if (file == m.file) return (byte)m.masters.Count;
            return m.masterIndices[file.filename];
        }

        public static bool HasMaster(this IMasterManager m, PluginFile file) {
            foreach (var master in m.masters)
                if (master == file) return true;
            return file == m.file;
        }

        public static void AddMaster(this IMasterManager m, PluginFile file) {
            if (m.HasMaster(file)) return;
            m.masters.Add(file);
        }

        public static void RemoveMaster(this IMasterManager m, PluginFile file) {
            m.masters.RemoveAll(p => p == file);
        }

        public static Dictionary<string, List<MainRecord>> GetMasterReferences(this IMasterManager m) {
            // TODO
            return null;
        }

        public static Dictionary<string, int> CountMasterReferences(this IMasterManager m) {
            // TODO
            return null;
        }

        public static List<string> GetUnusedMasters(this IMasterManager m) {
            // TODO
            return null;
        }

        public static void RemoveUnusedMasters(this IMasterManager m) {
            // TODO
        }
    }
}
