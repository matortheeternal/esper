using esper.elements;
using System;
using System.Threading;

namespace esperlib {
    public class RefThread {
        private static Thread activeThread = null;
        private static Element element;

        private static void Execute() {
            try {
                if (element is RootElement) {
                    BuildAllRefBy();
                } else {
                    BuildElementRefBy();
                }
                Meta.session.logger.Info("Done building references.");
                Meta.loaderState = LoaderState.Done;
            } catch (Exception e) {
                Meta.session.logger.Error($"Fatal Error: {e.Message}");
                Meta.loaderState = LoaderState.Error;
            } finally {
                activeThread = null;
            }
        }

        private static void BuildAllRefBy() {
            var root = element as RootElement;
            var total = root.count;
            for (int i = 0; i < total; i++) {
                var file = root.elements[i];
                Meta.session.logger.Info(
                    $"Building references for {file.name} ({i + 1}/{total})"
                );
            }
        }

        private static void BuildElementRefBy() {
            Meta.session.logger.Info($"Building references for {element.path}");
            element.BuildRefBy();
        }

        public static void Run(uint id, bool sync) {
            if (activeThread != null) 
                throw new Exception("Only one build references thread can be active at a time.");
            element = (Element) Meta.Resolve(id, true);
            activeThread = new Thread(new ThreadStart(Execute));
            Meta.loaderState = LoaderState.Active;
            if (sync) activeThread.Join();
        }
    }
}
