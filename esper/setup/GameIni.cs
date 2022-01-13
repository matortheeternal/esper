using IniParser;
using IniParser.Model;
using System;
using System.Collections.Generic;
using System.IO;

namespace esper.setup {
    public class GameIni {
        private readonly IniData iniData;

        public GameIni(Game game) {
            var ini = new FileIniDataParser();
            if (!File.Exists(game.iniPath))
                throw new Exception("Game ini file not found.  You may need to run the game launcher to initialize it.");
            iniData = ini.ReadFile(game.iniPath);
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
    }
}
