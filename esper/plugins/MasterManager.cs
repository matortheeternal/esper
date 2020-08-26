using esper.elements;
using esper.setup;
using esper.resolution;
using System.Collections.Generic;
using System;
using esper.data;

namespace esper.plugins {
    using FileReferenceMap = Dictionary<string, List<Element>>;

    public interface IMasterManager {
        public PluginFile file { get; }
        public ReadOnlyMasterList originalMasters { get; internal set; }
        public MasterList masters { get; internal set; }
        public bool mastersChanged { get; internal set; }
    }

    public static class MasterManagerExtensions {
        internal static Element GetMastersElement(this IMasterManager m) {
            if (m.file.header == null) return null;
            return m.file.header.GetElement("Master Files");
        }

        private static List<PluginFile> GetMasterFiles(this IMasterManager m) {
            var masterFiles = new List<PluginFile>();
            Element masterFilesElement = m.GetMastersElement();
            if (masterFilesElement == null) return masterFiles;
            PluginManager manager = m.file.session.pluginManager;
            var masterElements = masterFilesElement.GetElements();
            foreach (var element in masterElements) {
                string filename = element.GetValue("MAST");
                masterFiles.Add(manager.GetFileByName(filename, true));
            }
            return masterFiles;
        }

        public static void InitMasters(this IMasterManager m) {
            var masterFiles = GetMasterFiles(m);
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
            if (m.masters == null) return null;
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
            m.mastersChanged = true;
            m.masters.Add(file);
        }

        public static void RemoveMaster(this IMasterManager m, PluginFile file) {
            m.mastersChanged = true;
            m.masters.Remove(file);
        }

        private static void ForEachFormIdElement(
            PluginFile file, Action<ValueElement> callback
        ) {
            file.ForEachElement((Element element) => {
                if (element is Container)
                    return element.def.canContainFormIds;
                if (element is ValueElement v && v.def.canContainFormIds)
                    callback(v);
                return false;
            });
        }

        public static FileReferenceMap GetMasterReferences(
            this IMasterManager m
        ) {
            var refs = new FileReferenceMap();
            ForEachFormIdElement(m.file, (ValueElement element) => {
                if (!(element.data is FormId formId)) return;
                var filename = formId.targetFileName;
                if (!refs.ContainsKey(filename))
                    refs[filename] = new List<Element>();
                refs[filename].Add(element);
            });
            return refs;
        }

        public static Dictionary<string, int> CountMasterReferences(
            this IMasterManager m
        ) {
            var counts = new Dictionary<string, int>();
            ForEachFormIdElement(m.file, (ValueElement element) => {
                if (!(element.data is FormId formId)) return;
                var filename = formId.targetFileName;
                if (!counts.ContainsKey(filename))
                    counts[filename] = 0;
                counts[filename]++;
            });
            return counts;
        }

        public static List<string> GetUnusedMasters(this IMasterManager m) {
            // TODO
            return null;
        }

        public static void RemoveUnusedMasters(this IMasterManager m) {
            // TODO
        }

        public static void CheckMasters(this IMasterManager m) {
            if (m.masters.Count > 255)
                throw new Exception("File master limit exceeded.");
        }
    }
}
