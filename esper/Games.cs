namespace esper {
    [JSExport]
    public static class Games {
        public static Game TES4 = new Game {
            xeditId = 1,
            name = "Oblivion",
            fullName = "The Elder Scrolls IV: Oblivion",
            abbreviation = "TES4",
            steamAppIds = { 22330, 900883 }
        }.InitDefaults(); 

        public static Game FO3 = new Game {
            xeditId = 2,
            name = "Fallout 3",
            abbreviation = "FO3",
            iniName = "Fallout.ini",
            steamAppIds = { 22300, 22370 }
        }.InitDefaults();

        public static Game FNV = new Game("FalloutNV") {
            xeditId = 2,
            name = "Fallout NV",
            fullName = "Fallout: New Vegas",
            abbreviation = "FNV",
            registryName = "Fallout New Vegas",
            iniName = "Fallout.ini",
            steamAppIds = { 22380, 2028016 }
        }.InitDefaults();

        public static Game TES5 = new Game {
            xeditId = 4,
            name = "Skyrim",
            fullName = "The Elder Scrolls V: Skyrim",
            abbreviation = "TES5",
            exeName = "TESV.exe",
            hardcodedPlugins = { "Update.esm" },
            steamAppIds = { 72850 }
        }.InitDefaults();

        public static Game FO4 = new Game("Fallout4") {
            xeditId = 6,
            name = "Fallout 4",
            abbreviation = "FO4",
            cccName = "Fallout4.ccc",
            pluginsTxtType = PluginsTxtType.Asterisk,
            archiveExtension = ".ba2",
            extendedArchiveMatching = true,
            pluginExtensions = { 
                ModuleExtension.ESL,
                ModuleExtension.ESP,
                ModuleExtension.ESM
            },
            hardcodedPlugins = {
                "DLCRobot.esm",
                "DLCworkshop01.esm",
                "DLCCoast.esm",
                "DLCworkshop02.esm",
                "DLCworkshop03.esm",
                "DLCNukaWorld.esm",
                "DLCUltraHighResolution.esm"
            },
            steamAppIds = { 377160 }
        }.InitDefaults();

        public static Game SSE = new Game("Skyrim Special Edition", "Skyrim") {
            xeditId = 7,
            name = "Skyrim SE",
            fullName = "Skyrim: Special Edition",
            abbreviation = "SSE",
            defsNamespace = "TES5",
            cccName = "Skyrim.ccc",
            pluginsTxtType = PluginsTxtType.Asterisk,
            extendedArchiveMatching = true,
            pluginExtensions = { 
                ModuleExtension.ESL,
                ModuleExtension.ESP,
                ModuleExtension.ESM
            },
            hardcodedPlugins = {
                "Update.esm",
                "Dawnguard.esm",
                "HeathFires.esm",
                "Dragonborn.esm"
            },
            steamAppIds = { 377160 }
        }.InitDefaults();

        public static Game SF = new Game {
            xeditId = 9,
            name = "Starfield",
            abbreviation = "SF",
            pluginExtensions = {
                ModuleExtension.ESL,
                ModuleExtension.ESP,
                ModuleExtension.ESM
            },
            hardcodedPlugins = {
                "BlueprintShips-Starfield.esm",
                "OldMars.esm",
                "Constellation.esm",
            },
            steamAppIds = { 377160 }
        }.InitDefaults();
    }
}
