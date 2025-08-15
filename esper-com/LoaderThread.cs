using System;
using System.Threading;

namespace esperlib {
    public class LoaderThread {
        private static string loadOrder;
        private static bool smartLoad;
        private static Thread activeThread = null;

        private static void Execute() {
            try {
                LoadResources();
                LoadPluginFiles();
                Meta.session.logger.Info("Done loading files.");
                Meta.loaderState = LoaderState.Done;
            } catch (Exception e) {
                Meta.session.logger.Error($"Fatal Error: {e.Message}");
                Meta.loaderState = LoaderState.Error;
            } finally {
                activeThread = null;
            }
        }

        private static void LoadResources() {
            Meta.session.resourceManager.LoadArchives();
        }

        private static void LoadPluginFiles() {
            foreach (string entry in loadOrder.Split("\n"))
                Meta.session.pluginManager.LoadPlugin(entry);
        }

        public static void Run(string loadOrder, bool smartLoad) {
            if (activeThread != null) 
                throw new Exception("Only one loader thread can be active at a time.");
            LoaderThread.loadOrder = loadOrder;
            LoaderThread.smartLoad = smartLoad;
            activeThread = new Thread(new ThreadStart(Execute));
            Meta.loaderState = LoaderState.Active;
        }
    }
}
