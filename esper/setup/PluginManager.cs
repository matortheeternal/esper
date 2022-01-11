using esper.data;
using esper.elements;
using esper.plugins;
using System;
using System.Collections.Generic;
using System.IO;

namespace esper.setup {
    public class PluginManager {
        public Game game;
        public Session session;
        public RootElement root;
        public bool usingLightPlugins;
        public int maxLightPluginIndex;
        public int maxFullPluginIndex;
        public List<PluginFile> plugins;
        public List<FullPluginSlot> fullPluginSlots;
        public List<LightPluginSlot> lightPluginSlots;

        public Logger logger => session.logger;

        public int nextLightPluginIndex {
            get {
                int index = lightPluginSlots.Count;
                if (index > maxLightPluginIndex)
                    throw new Exception("Maximum light plugins exceeded.");
                return index;
            }
        }

        public int nextFullPluginIndex {
            get {
                int index = fullPluginSlots.Count;
                if (index > maxFullPluginIndex)
                    throw new Exception("Maximum full plugins exceeded.");
                return index;
            }
        }

        public PluginManager(Game game, Session session) {
            this.game = game;
            this.session = session;
            usingLightPlugins = session.options.allowLightPlugins &&
                game.SupportsLightPlugins();
            maxLightPluginIndex = usingLightPlugins ? 4095 : 0;
            maxFullPluginIndex = usingLightPlugins ? 253 : 254;
            plugins = new List<PluginFile>();
            fullPluginSlots = new List<FullPluginSlot>();
            lightPluginSlots = new List<LightPluginSlot>();
            root = new RootElement(session);
        }

        public bool ShouldUseLightPluginSlot(PluginFile plugin) {
            return usingLightPlugins && plugin.esl;
        }

        public void AssignLoadOrder(PluginFile plugin) {
            if (ShouldUseLightPluginSlot(plugin)) {
                lightPluginSlots.Add(
                    new LightPluginSlot(plugin, nextLightPluginIndex)
                );
            } else {
                fullPluginSlots.Add(
                    new FullPluginSlot(plugin, nextFullPluginIndex)
                );
            }
        }

        public void AddFile(PluginFile plugin) {
            if (plugin.options.temporary) return;
            plugins.Add(plugin);
            if (!session.options.emulateGlobalLoadOrder) return;
            AssignLoadOrder(plugin);
        }

        public PluginFile CreateDummyPlugin(string filename) {
            logger.Info($"Using dummy plugin for {filename}");
            return new PluginFile(session, filename, new PluginFileOptions{});
        }

        public PluginFile GetFileByName(
            string filename, 
            bool createDummyIfMissing = false
        ) {
            foreach (var plugin in plugins)
                if (plugin.filename == filename) return plugin;
            if (!createDummyIfMissing) return null;
            return CreateDummyPlugin(filename);
        }

        public PluginFile LoadPluginHeader(string filePath) {
            var options = new PluginFileOptions { temporary = true };
            var filename = Path.GetFileName(filePath);
            PluginFile plugin = new PluginFile(session, filename, options);
            new PluginFileSource(filePath, plugin);
            plugin.pluginDef.ReadFileHeader(plugin);
            return plugin;
        }

        public PluginFile LoadPlugin(string filePath) {
            var options = new PluginFileOptions();
            var filename = Path.GetFileName(filePath);
            logger.Info($"Loading plugin {filename}");
            PluginFile plugin = new PluginFile(session, filename, options);
            new PluginFileSource(filePath, plugin);
            plugin.container = root;
            plugin.pluginDef.ReadFileHeader(plugin);
            plugin.pluginDef.ReadGroups(plugin);
            return plugin;
        }

        public List<string> GetMasterFileNames(string filePath) {
            var plugin = LoadPluginHeader(filePath);
            return (plugin as IMasterManager).masters.filenames;
        }

        internal PluginFile NewFile(string name) {
            throw new NotImplementedException();
        }

        internal UInt32? GetGlobalFormId(PluginFile file, UInt32 fileFormId) {
            var formId = FormId.FromSource(file, fileFormId);
            return formId.globalFormId;
        }
    }
}
