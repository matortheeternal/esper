using esper.elements;
using esper.setup;
using System.Collections.Generic;

namespace esper.plugins {
    public class MasterManager {
        public List<PluginFile> masters;
        public List<PluginFile> userMasters;
        public Dictionary<string, byte> masterIndices;
        public Dictionary<string, byte> userMasterIndices;
        public PluginFile file { get;  }

        public MasterManager() {}

        public Element GetMastersElement() {
            if (file.header == null) return null;
            return file.header.GetElement("Master Files");
        }

        public void InitMasters() {
            Element masterFilesElement = GetMastersElement();
            if (masterFilesElement == null) return;
            PluginManager manager = file.session.pluginManager;
            List<Element> masterElements = masterFilesElement.GetElements();
            foreach (var element in masterElements) {
                string filename = element.GetValue("MAST");
                masters.Add(manager.GetFileByName(filename, true));
            }
            userMasters.AddRange(masters);
        }
    }
}
