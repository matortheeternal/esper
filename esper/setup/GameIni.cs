using IniParser;
using IniParser.Model;

namespace esper.setup {
    //[JSExport]
    public class GameIni {
        private static readonly string[] TES4Languages = {
            null, "German", "French", "Spanish", "Italian"
        };

        private readonly IniData iniData;
        private readonly Session session;

        public GameIni(Session session) : this(session, session.game.iniPath) { }

        internal GameIni(Session session, string iniPath) {
            this.session = session;
            if (File.Exists(iniPath)) {
                var ini = new FileIniDataParser();
                iniData = ini.ReadFile(iniPath);
                return;
            }
            session.logger.Error(
                "Game ini file not found.  You may need to run the game " +
                "launcher to initialize it."
            );
        }

        private void AddArchives(
            List<string> archivePaths, KeyDataCollection archiveSection,
            string key
        ) {
            var value = archiveSection[key];
            if (value == null) return;
            archivePaths.AddRange(
                value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(path => path.Trim())
                    .Where(path => path.Length > 0)
            );
        }

        public List<string> GetArchives() {
            var archives = new List<string>();
            var archiveSection = iniData?["Archive"];
            if (archiveSection == null) return archives;
            AddArchives(archives, archiveSection, "sResourceArchiveList");
            AddArchives(archives, archiveSection, "sResourceArchiveList2");
            AddArchives(archives, archiveSection, "sArchiveToLoadInMemoryList");
            return archives;
        }

        public string GetConfiguredLanguage() {
            if (iniData == null) return null;
            if (string.Equals(
                session.game.abbreviation, "TES4",
                StringComparison.OrdinalIgnoreCase
            )) {
                var configuredIndex = iniData["Controls"]?["iLanguage"];
                if (!Int32.TryParse(configuredIndex, out var languageIndex))
                    return "English";
                if (languageIndex > 0 && languageIndex < TES4Languages.Length) {
                    return TES4Languages[languageIndex];
                }
            } else {
                return iniData["General"]?["sLanguage"];
            }
            return "English";
        }
    }
}
