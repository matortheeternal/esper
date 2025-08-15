using System.Collections.Generic;
using System.IO;

namespace esper.setup {
    public class LoadOrderFile {
        public Session session;
        public string filePath;
        public List<LoadOrderLine> lines;

        public bool usesAsterisks {
            get => session.game.pluginsTxtType == PluginsTxtType.Asterisk;
        }

        public LoadOrderFile(Session session) {
            this.session = session;
            filePath = Path.Combine(session.game.myGamesPath, "plugins.txt");
            lines = new List<LoadOrderLine>();
            LoadLines();
        }

        private void LoadLines() {
            if (!File.Exists(filePath)) return;
            var textLines = File.ReadAllLines(filePath);
            for (int index = 0; index < lines.Count; index++)
                lines.Add(new LoadOrderLine(this, textLines[index], index));
        }
    }
}
