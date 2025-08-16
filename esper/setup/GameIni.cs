using IniParser;
using IniParser.Model;

namespace esper.setup {
    [JSExport]
    public class GameIni {
        private static readonly string[] TES4Languages = {
            null, "German", "French", "Spanish", "Italian"
        };

        private readonly IniData iniData;
        private readonly Session session;

        public GameIni(Session session) {
            this.session = session;
            if (File.Exists(session.game.iniPath)) {
                var ini = new FileIniDataParser();
                iniData = ini.ReadFile(session.game.iniPath);
                return;
            }
            session.logger.Error(
                "Game ini file not found.  You may need to run the game " +
                "launcher to initialize it."
            );
        }

        private void AddArchives(List<string> archivePaths, string key) {
            var value = iniData["Archive"][key];
            if (value == null) return;
            archivePaths.AddRange(value.Split(", "));
        }

        public List<string> GetArchives() {
            if (iniData["Archive"] == null) 
                throw new Exception("Archive section not found in game INI");
            var archives = new List<string>();
            AddArchives(archives, "sResourceAchiveList");
            AddArchives(archives, "sResourceAchiveList2");
            AddArchives(archives, "sArchiveToLoadInMemoryList");
            return archives;
        }

        public string GetConfiguredLanguage() {
            if (iniData == null) return null;
            if (session.game == Games.TES4) {
                var languageIndex = Int32.Parse(iniData["Controls"]["iLanguage"]);
                if (languageIndex > 0 && languageIndex < TES4Languages.Length) {
                    return TES4Languages[languageIndex];
                }
            } else {
                return iniData["General"]["sLanguage"];
            }
            return "English";
        }
    }
}
