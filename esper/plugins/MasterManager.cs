using esper.elements;
using esper.setup;
using System.Collections.Generic;

namespace esper.plugins {
    using FileReferenceMap = Dictionary<string, List<MainRecord>>;

    public interface IMasterManager {
        public PluginFile file { get; }
        public ReadOnlyMasterList originalMasters { get; internal set; }
        public MasterList masters { get; internal set; }
    }

    public static class MasterManagerExtensions {
        internal static Element GetMastersElement(this IMasterManager m) {
            if (m.file.header == null) return null;
            return m.file.header.GetElement("Master Files");
        }

        public static void InitMasters(this IMasterManager m) {
            var masterFiles = new List<PluginFile>();
            Element masterFilesElement = m.GetMastersElement();
            if (masterFilesElement == null) return;
            PluginManager manager = m.file.session.pluginManager;
            List<Element> masterElements = masterFilesElement.GetElements();
            foreach (var element in masterElements) {
                string filename = element.GetValue("MAST");
                masterFiles.Add(manager.GetFileByName(filename, true));
            }
            m.originalMasters = new ReadOnlyMasterList(m.file, masterFiles);
            m.masters = new MasterList(m.file, masterFiles);
        }

        public static void UpdateMastersElement(this IMasterManager m) {
            Element masterFilesElement = m.GetMastersElement();
            if (masterFilesElement == null) return;
            // TODO
        }

        public static PluginFile OrdinalToFile(
            this IMasterManager m, byte ordinal, bool useCurrentMasters
        ) {
            return useCurrentMasters
                ? m.masters.OrdinalToFile(ordinal)
                : m.originalMasters.OrdinalToFile(ordinal);
        }

        public static byte FileToOrdinal(
            this IMasterManager m, PluginFile file, bool useCurrentMasters
        ) {
            return useCurrentMasters
                ? m.masters.FileToOrdinal(file)
                : m.originalMasters.FileToOrdinal(file);
        }

        public static bool HasMaster(this IMasterManager m, PluginFile file) {
            return m.masters.Contains(file);
        }

        public static void AddMaster(this IMasterManager m, PluginFile file) {
            if (m.HasMaster(file)) return;
            m.masters.Add(file);
        }

        public static void RemoveMaster(this IMasterManager m, PluginFile file) {
            m.masters.Remove(file);
        }

        public static FileReferenceMap GetMasterReferences(
            this IMasterManager m
        ) {
            // TODO
            return null;
        }

        public static Dictionary<string, int> CountMasterReferences(
            this IMasterManager m
        ) {
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
