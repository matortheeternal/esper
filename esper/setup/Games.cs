namespace esper {
    public static class Games {
        public static Game TES4 = new Game {
            xeditId = 1,
            name = "Oblivion",
            baseName = "Oblivion",
            fullName = "The Elder Scrolls IV: Oblivion",
            abbreviation = "TES4",
            defsNamespace = "TES4",
            registryName = "Oblivion",
            myGamesFolderName = "Oblivion",
            appDataFolderName = "Oblivion",
            exeName = "Oblivion.exe",
            esmName = "Oblivion.esm",
            iniName = "Oblivion.ini",
            cccName = null,
            pluginsTxtType = "plain",
            archiveExtension = ".bsa",
            pluginExtensions = new string[] { ".esp", ".esm" },
            hardcodedPlugins = new string[] {
                "Oblivion.esm"
            },
            steamAppIds = new int[] { 22330, 900883 }
        };

        public static Game FO3 = new Game {
            xeditId = 2,
            name = "Fallout 3",
            baseName = "Fallout3",
            fullName = "Fallout 3",
            abbreviation = "FO3",
            defsNamespace = "FO3",
            registryName = "Fallout 3",
            myGamesFolderName = "Fallout 3",
            appDataFolderName = "Fallout 3",
            exeName = "Fallout3.exe",
            esmName = "Fallout3.esm",
            iniName = "Fallout.ini",
            cccName = null,
            pluginsTxtType = "plain",
            archiveExtension = ".bsa",
            pluginExtensions = new string[] { ".esp", ".esm" },
            hardcodedPlugins = new string[] {
                "Fallout3.esm"
            },
            steamAppIds = new int[] { 22300, 22370 }
        };

        public static Game FNV = new Game {
            xeditId = 2,
            name = "Fallout NV",
            baseName = "FalloutNV",
            fullName = "Fallout: New Vegas",
            abbreviation = "FNV",
            defsNamespace = "FNV",
            registryName = "Fallout New Vegas",
            myGamesFolderName = "FalloutNV",
            appDataFolderName = "FalloutNV",
            exeName = "FalloutNV.exe",
            esmName = "FalloutNV.esm",
            iniName = "Fallout.ini",
            cccName = null,
            pluginsTxtType = "plain",
            archiveExtension = ".bsa",
            pluginExtensions = new string[] { ".esp", ".esm" },
            hardcodedPlugins = new string[] {
                "FalloutNV.esm"
            },
            steamAppIds = new int[] { 22380, 2028016 }
        };

        public static Game TES5 = new Game {
            xeditId = 4,
            name = "Skyrim",
            baseName = "Skyrim",
            fullName = "The Elder Scrolls V: Skyrim",
            abbreviation = "TES5",
            defsNamespace = "TES5",
            registryName = "Skyrim",
            myGamesFolderName = "Skyrim",
            appDataFolderName = "Skyrim",
            exeName = "TESV.exe",
            esmName = "Skyrim.esm",
            iniName = "Skyrim.ini",
            cccName = null,
            pluginsTxtType = "plain",
            archiveExtension = ".bsa",
            pluginExtensions = new string[] { ".esp", ".esm" },
            hardcodedPlugins = new string[] {
                "Skyrim.esm",
                "Update.esm"
            },
            steamAppIds = new int[] { 72850 }
        };

        public static Game FO4 = new Game {
            xeditId = 6,
            name = "Fallout 4",
            baseName = "Fallout4",
            fullName = "Fallout 4",
            abbreviation = "FO4",
            defsNamespace = "FO4",
            registryName = "Fallout4",
            myGamesFolderName = "Fallout4",
            appDataFolderName = "Fallout4",
            exeName = "Fallout4.exe",
            esmName = "Fallout4.esm",
            iniName = "Fallout4.ini",
            cccName = "Fallout4.ccc",
            pluginsTxtType = "asterisk",
            archiveExtension = ".ba2",
            pluginExtensions = new string[] { ".esp", ".esm", ".esl" },
            hardcodedPlugins = new string[] {
                "Fallout4.esm",
                "DLCRobot.esm",
                "DLCworkshop01.esm",
                "DLCCoast.esm",
                "DLCworkshop02.esm",
                "DLCworkshop03.esm",
                "DLCNukaWorld.esm",
                "DLCUltraHighResolution.esm"
            },
            steamAppIds = new int[] { 377160 }
        };

        public static Game SSE = new Game {
            xeditId = 7,
            name = "Skyrim SE",
            baseName = "SkyrimSE",
            fullName = "Skyrim: Special Edition",
            abbreviation = "SSE",
            defsNamespace = "TES5",
            registryName = "Skyrim Special Edition",
            myGamesFolderName = "Skyrim Special Edition",
            appDataFolderName = "Skyrim Special Edition",
            exeName = "SkyrimSE.exe",
            esmName = "Skyrim.esm",
            iniName = "Skyrim.ini",
            cccName = "Skyrim.ccc",
            pluginsTxtType = "asterisk",
            archiveExtension = ".bsa",
            pluginExtensions = new string[] { ".esp", ".esm", ".esl" },
            hardcodedPlugins = new string[] {
                "Skyrim.esm",
                "Update.esm",
                "Dawnguard.esm",
                "HeathFires.esm",
                "Dragonborn.esm"
            },
            steamAppIds = new int[] { 377160 }
        };
    }
}
