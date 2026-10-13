using esper;
using esper.helpers;
using esper.setup;
using NUnit.Framework;
using System;
using System.IO;
using Tests;
using Tests.defs;

namespace Tests.setup {
    public class SessionOptionsTests {
        [Test]
        public void TestDefaults() {
            var options = new SessionOptions();
            Assert.IsFalse(options.keepMasterElementsUpdated);
            Assert.IsTrue(options.allowLightPlugins);
            Assert.IsTrue(options.emulateGlobalLoadOrder);
            Assert.AreEqual("English", options.language);
            Assert.IsFalse(options.readAllSubrecords);
            Assert.IsTrue(options.clampIntegerValues);
            Assert.IsFalse(options.resolveAliases);
            Assert.IsFalse(options.enforceExpectedReferences);
            Assert.IsFalse(options.loadResources);
            Assert.IsFalse(options.improvise);
            Assert.IsFalse(options.strictDefinitions);
            Assert.IsFalse(options.serializeNumericData);
            Assert.AreEqual(6u, options.floatDigits);
            Assert.AreEqual("F6", options.floatFormat);
            Assert.AreEqual("", options.gamePath);
        }

        [Test]
        public void TestDefaultEpsilon() {
            var options = new SessionOptions();
            // just under 1e-6: tolerance for 6-digit float rounding
            Assert.AreEqual(1e-6, options.epsilon, 1e-12);
        }

        [Test]
        public void TestFloatDigitsUpdatesFormatAndEpsilon() {
            var options = new SessionOptions();
            var defaultEpsilon = options.epsilon;
            options.floatDigits = 2;
            Assert.AreEqual("F2", options.floatFormat);
            Assert.AreEqual(1e-2, options.epsilon, 1e-9);
            Assert.AreNotEqual(defaultEpsilon, options.epsilon);
        }

        [Test]
        public void TestEncodingLookupIsCaseInsensitive() {
            var options = new SessionOptions { language = "english" };
            Assert.AreEqual(1252, options.encoding.CodePage);
        }
    }

    public class GameFactsTests {
        [Test]
        public void TestSSEFacts() {
            Assert.AreEqual("SSE", Games.SSE.abbreviation);
            Assert.AreEqual("TES5", Games.SSE.defsNamespace);
            Assert.IsTrue(Games.SSE.SupportsLightPlugins());
            Assert.AreEqual(
                PluginsTxtType.Asterisk, Games.SSE.pluginsTxtType
            );
            // esmName is inserted at index 0 by the Game constructor
            Assert.AreEqual("Skyrim.esm", Games.SSE.hardcodedPlugins[0]);
            CollectionAssert.Contains(
                Games.SSE.hardcodedPlugins, "Dragonborn.esm"
            );
        }

        [Test]
        public void TestTES5Facts() {
            Assert.AreEqual("TES5", Games.TES5.abbreviation);
            Assert.IsFalse(Games.TES5.SupportsLightPlugins());
            Assert.AreEqual(PluginsTxtType.Plain, Games.TES5.pluginsTxtType);
            Assert.AreEqual("Skyrim.esm", Games.TES5.hardcodedPlugins[0]);
            Assert.AreEqual("Update.esm", Games.TES5.hardcodedPlugins[1]);
        }

        [Test]
        public void TestFO4Facts() {
            Assert.AreEqual(".ba2", Games.FO4.archiveExtension);
            Assert.IsTrue(Games.FO4.extendedArchiveMatching);
            Assert.IsTrue(Games.FO4.SupportsLightPlugins());
        }

        [Test]
        public void TestGameNameOverride() {
            var game = new Game("Skyrim Special Edition", "Skyrim");
            Assert.AreEqual("Skyrim Special Edition", game.registryName);
            Assert.AreEqual("Skyrim Special Edition", game.myGamesFolderName);
        }

        [TestCase("TES4", "Oblivion.ini")]
        [TestCase("FO3", "Fallout.ini")]
        [TestCase("FNV", "Fallout.ini")]
        [TestCase("TES5", "Skyrim.ini")]
        [TestCase("FO4", "Fallout4.ini")]
        [TestCase("SSE", "Skyrim.ini")]
        [TestCase("SF", "Starfield.ini")]
        public void TestIniPathHasOneExtension(string abbreviation, string expected) {
            var game = RecordDefCoverageTests.games[abbreviation];
            Assert.AreEqual(expected, Path.GetFileName(game.iniPath));
        }

        [Test]
        public void TestAppDataPathUsesLocalApplicationData() {
            Assert.AreEqual(
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.LocalApplicationData
                    ),
                    Games.SSE.appDataFolderName
                ),
                Games.SSE.appDataPath
            );
        }
    }

    public class SessionLifecycleTests {
        [Test]
        public void TestSessionLogsStartupMessages() {
            var session = Synthetic.GetSession(Games.SSE);
            var messages = string.Join("\n", session.logger.messages);
            StringAssert.Contains("Esper v", messages);
            StringAssert.Contains(
                "Starting a new session for Skyrim SE", messages
            );
        }

        [Test]
        public void TestMissingGameIniLogsError() {
            var session = Synthetic.GetSession(Games.SSE);
            var missingPath = Path.Combine(
                Path.GetTempPath(), Guid.NewGuid().ToString("N"), "Skyrim.ini"
            );
            var gameIni = new GameIni(session, missingPath);
            Assert.IsFalse(File.Exists(missingPath));
            var messages = string.Join("\n", session.logger.messages);
            StringAssert.Contains("Game ini file not found", messages);
            Assert.IsNull(gameIni.GetConfiguredLanguage());
            Assert.IsEmpty(gameIni.GetArchives());
        }

        [Test]
        public void TestGamePathUsesInstallationRootDataFolder() {
            var gameRoot = Path.Combine(
                Path.GetTempPath(), Guid.NewGuid().ToString("N")
            );
            var session = new Session(Games.SSE, new SessionOptions {
                gamePath = gameRoot
            });
            Assert.AreEqual(Path.Combine(gameRoot, "Data"), session.dataPath);
        }
    }

    public class PluginManagerLifecycleTests {
        private Session session;

        [SetUp]
        public void SetUp() {
            session = Synthetic.GetSession(Games.SSE);
        }

        [Test]
        public void TestCreateDummyPluginLifecycle() {
            var plugin = session.pluginManager.CreateDummyPlugin("Dummy.esp");
            Assert.IsNotNull(plugin);
            Assert.IsTrue(session.pluginManager.HasPlugin("Dummy.esp"));
            Assert.AreSame(
                plugin, session.pluginManager.GetFileByName("Dummy.esp")
            );
            session.pluginManager.UnloadPlugin(plugin);
            Assert.IsFalse(session.pluginManager.HasPlugin("Dummy.esp"));
        }

        [Test]
        public void TestPluginLookupIsCaseInsensitive() {
            var plugin = session.pluginManager.CreateDummyPlugin("MixedCase.ESP");
            Assert.AreSame(
                plugin, session.pluginManager.GetFileByName("mixedcase.esp")
            );
            Assert.IsTrue(session.pluginManager.HasPlugin("MIXEDCASE.ESP"));
            session.pluginManager.UnloadPlugin(plugin);
        }

        [Test]
        public void TestGetMasterFileNames() {
            var masters = session.pluginManager.GetMasterFileNames(
                TestHelpers.FixturePath("EmptyPlugin.esp")
            );
            CollectionAssert.AreEqual(
                new[] { "Skyrim.esm" }, masters
            );
        }

        [Test]
        public void TestLoadPluginHeader() {
            var plugin = session.pluginManager.LoadPluginHeader(
                TestHelpers.FixturePath("AllRecords.esp")
            );
            try {
                Assert.AreEqual("AllRecords.esp", plugin.filename);
                Assert.IsNotNull(plugin.header);
            } finally {
                session.pluginManager.UnloadPlugin(plugin);
            }
        }
    }

    public class PluginSlotTests {
        private Session session;
        private esper.plugins.PluginFile plugin;

        [SetUp]
        public void SetUp() {
            session = Synthetic.GetSession(Games.SSE);
            plugin = session.pluginManager.CreateDummyPlugin("Slot.esp");
        }

        [TearDown]
        public void TearDown() {
            session.pluginManager.UnloadPlugin(plugin);
        }

        [Test]
        public void TestFullPluginSlot() {
            var slot = new FullPluginSlot(plugin, 2);
            Assert.AreEqual(0x02000000u, slot.GetOrdinal());
            Assert.AreEqual(0x02001234u, slot.FormatFormId(0x1234));
            Assert.Throws<Exception>(() => slot.FormatFormId(0xFF000001));
        }

        [Test]
        public void TestLightPluginSlot() {
            var slot = new LightPluginSlot(plugin, 5);
            Assert.AreEqual(
                0xFE000000u | ((uint)5 << 12), slot.GetOrdinal()
            );
            Assert.AreEqual(slot.GetOrdinal() + 0xABCu, slot.FormatFormId(0xABC));
            Assert.Throws<Exception>(() => slot.FormatFormId(0x1000));
        }
    }

    public class ModuleInfoTests {
        [Test]
        public void TestEmptyPluginModuleInfo() {
            var session = Synthetic.GetSession(Games.SSE);
            try {
                var module = new ModuleInfo(
                    session, TestHelpers.FixturePath("EmptyPlugin.esp")
                );
                Assert.AreEqual("EmptyPlugin.esp", module.name);
                Assert.AreEqual(ModuleExtension.ESP, module.extension);
                // fixed: flags are OR-assigned, Valid is set
                Assert.AreEqual(
                    ModuleFlags.Valid, module.flags & ModuleFlags.Valid
                );
                Assert.IsFalse(module.HasMissingMasters);
                Assert.IsNotNull(module.plugin);
            } finally {
                foreach (var plugin in session.pluginManager.plugins.ToArray())
                    session.pluginManager.UnloadPlugin(plugin);
            }
        }

        [Test]
        public void TestModuleInfoCompareTo() {
            var session = Synthetic.GetSession(Games.SSE);
            try {
                var a = new ModuleInfo(
                    session, TestHelpers.FixturePath("EmptyPlugin.esp")
                );
                var b = new ModuleInfo(
                    session, TestHelpers.FixturePath("AllRecords.esp")
                );
                Assert.AreEqual(0, a.CompareTo(b));
                a.officialIndex = 0;
                b.officialIndex = 1;
                Assert.Less(a.CompareTo(b), 0);
                Assert.Greater(b.CompareTo(a), 0);
            } finally {
                foreach (var plugin in session.pluginManager.plugins.ToArray())
                    session.pluginManager.UnloadPlugin(plugin);
            }
        }
    }

    public class LoadOrderFileTests {
        [Test]
        public void TestMissingPluginsTxtYieldsNoLines() {
            var session = Synthetic.GetSession(Games.SSE);
            var missingPath = Path.Combine(
                Path.GetTempPath(), Guid.NewGuid().ToString("N"), "plugins.txt"
            );
            var file = new LoadOrderFile(session, missingPath);
            Assert.IsFalse(File.Exists(file.filePath));
            Assert.IsEmpty(file.lines);
            Assert.IsTrue(file.usesAsterisks);
        }

        [Test]
        public void TestPluginsTxtUsesLocalAppData() {
            var session = Synthetic.GetSession(Games.SSE);
            var file = new LoadOrderFile(session);
            Assert.AreEqual(
                Path.Combine(Games.SSE.appDataPath, "plugins.txt"),
                file.filePath
            );
        }

        [Test]
        public void TestLoadOrderLineParsing() {
            var session = Synthetic.GetSession(Games.SSE);
            var file = new LoadOrderFile(session);
            var active = new LoadOrderLine(file, "*Skyrim.esm", 0);
            Assert.IsTrue(active.isActive);
            Assert.IsFalse(active.isComment);
            Assert.AreEqual("Skyrim.esm", active.moduleName);
            var inactive = new LoadOrderLine(file, "Foo.esp", 1);
            Assert.IsFalse(inactive.isActive);
            Assert.AreEqual("Foo.esp", inactive.moduleName);
            var comment = new LoadOrderLine(file, "# comment", 2);
            Assert.IsTrue(comment.isComment);
        }

        [Test]
        public void TestPlainPluginsTxtIgnoresAsterisks() {
            var session = Synthetic.GetSession(Games.TES5);
            var file = new LoadOrderFile(session);
            Assert.IsFalse(file.usesAsterisks);
            var line = new LoadOrderLine(file, "Skyrim.esm", 0);
            Assert.IsTrue(line.isActive);
            Assert.AreEqual("Skyrim.esm", line.moduleName);
        }
    }

    public class LoadOrderTests {
        [Test]
        public void TestMissingDataPathYieldsEmptyLoadOrderAndWarning() {
            var session = Synthetic.GetSession(Games.TES5);
            var loadOrder = new LoadOrder(session);
            Assert.IsEmpty(loadOrder.modules);
            StringAssert.Contains(
                "without a data path",
                string.Join("\n", session.logger.messages)
            );
        }

        [Test]
        public void TestMissingConfiguredDataPathYieldsEmptyLoadOrder() {
            var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            var session = new Session(Games.TES5, new SessionOptions {
                gamePath = root
            });
            var loadOrder = new LoadOrder(session);
            Assert.IsEmpty(loadOrder.modules);
            StringAssert.Contains(
                "data path not found",
                string.Join("\n", session.logger.messages).ToLowerInvariant()
            );
        }

        [Test]
        public void TestMissingCreationClubFileIsOptional() {
            var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(root, "Data"));
            try {
                var session = new Session(Games.SSE, new SessionOptions {
                    gamePath = root
                });
                var loadOrder = new LoadOrder(session);
                Assert.IsEmpty(loadOrder.creationClubPlugins);
                StringAssert.Contains(
                    "Creation Club plugin file not found",
                    string.Join("\n", session.logger.messages)
                );
            } finally {
                Directory.Delete(root, true);
            }
        }

        [Test]
        public void TestHardcodedModuleMatchingIsCaseInsensitive() {
            var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            var dataPath = Path.Combine(root, "Data");
            Directory.CreateDirectory(dataPath);
            File.Copy(
                TestHelpers.FixturePath("EmptyPlugin.esp"),
                Path.Combine(dataPath, "SKYRIM.ESM")
            );
            Session session = null;
            LoadOrder loadOrder = null;
            try {
                session = new Session(Games.TES5, new SessionOptions {
                    gamePath = root
                });
                loadOrder = new LoadOrder(session);
                Assert.AreEqual(1, loadOrder.modules.Count);
                Assert.AreNotEqual(
                    0,
                    loadOrder.modules[0].flags & ModuleFlags.IsGameMaster
                );
            } finally {
                if (session != null && loadOrder != null) {
                    foreach (var module in loadOrder.modules)
                        session.pluginManager.UnloadPlugin(module.plugin);
                    foreach (var plugin in session.pluginManager.plugins.ToArray())
                        session.pluginManager.UnloadPlugin(plugin);
                }
                Directory.Delete(root, true);
            }
        }

        [Test]
        public void TestMissingHardcodedModuleDoesNotSkipLaterModules() {
            var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            var dataPath = Path.Combine(root, "Data");
            Directory.CreateDirectory(dataPath);
            File.Copy(
                TestHelpers.FixturePath("EmptyPlugin.esp"),
                Path.Combine(dataPath, "UPDATE.ESM")
            );
            Session session = null;
            LoadOrder loadOrder = null;
            try {
                session = new Session(Games.TES5, new SessionOptions {
                    gamePath = root
                });
                loadOrder = new LoadOrder(session);
                Assert.AreEqual(1, loadOrder.modules.Count);
                Assert.AreEqual(1, loadOrder.modules[0].officialIndex);
                Assert.AreNotEqual(
                    0, loadOrder.modules[0].flags & ModuleFlags.Active
                );
            } finally {
                if (session != null && loadOrder != null) {
                    foreach (var module in loadOrder.modules)
                        session.pluginManager.UnloadPlugin(module.plugin);
                    foreach (var plugin in session.pluginManager.plugins.ToArray())
                        session.pluginManager.UnloadPlugin(plugin);
                }
                Directory.Delete(root, true);
            }
        }
    }

    public class GameIniTests {
        private static string WriteIni(string contents) {
            var path = Path.Combine(
                Path.GetTempPath(), $"EsperIni{Guid.NewGuid():N}.ini"
            );
            File.WriteAllText(path, contents);
            return path;
        }

        [Test]
        public void TestArchiveListsUseCorrectKeysAndFlexibleCommas() {
            var path = WriteIni(
                "[Archive]\n" +
                "sResourceArchiveList=Base.bsa, Textures.bsa\n" +
                "sResourceArchiveList2=Voices.bsa,Interface.bsa\n" +
                "sArchiveToLoadInMemoryList=Memory.bsa\n"
            );
            try {
                var session = Synthetic.GetSession(Games.TES5);
                var gameIni = new GameIni(session, path);
                CollectionAssert.AreEqual(
                    new[] {
                        "Base.bsa", "Textures.bsa", "Voices.bsa",
                        "Interface.bsa", "Memory.bsa"
                    },
                    gameIni.GetArchives()
                );
            } finally {
                File.Delete(path);
            }
        }

        [Test]
        public void TestMissingArchiveSectionYieldsEmptyList() {
            var path = WriteIni("[General]\nsLanguage=English\n");
            try {
                var session = Synthetic.GetSession(Games.SSE);
                Assert.IsEmpty(new GameIni(session, path).GetArchives());
            } finally {
                File.Delete(path);
            }
        }

        [Test]
        public void TestTES4LanguageUsesGameIdentityNotObjectReference() {
            var path = WriteIni("[Controls]\niLanguage=2\n");
            try {
                var session = Synthetic.GetSession(Games.TES4);
                Assert.AreEqual(
                    "French", new GameIni(session, path).GetConfiguredLanguage()
                );
            } finally {
                File.Delete(path);
            }
        }

        [Test]
        public void TestMalformedTES4LanguageFallsBackToEnglish() {
            var path = WriteIni("[Controls]\niLanguage=invalid\n");
            try {
                var session = Synthetic.GetSession(Games.TES4);
                Assert.AreEqual(
                    "English", new GameIni(session, path).GetConfiguredLanguage()
                );
            } finally {
                File.Delete(path);
            }
        }
    }

    public class ResourceManagerTests {
        [Test]
        public void TestNoArchivesRegisteredByDefault() {
            var session = Synthetic.GetSession(Games.SSE);
            Assert.IsEmpty(
                session.resourceManager.GetAssociatedArchives("Skyrim.esm")
            );
            Assert.DoesNotThrow(() => session.resourceManager.LoadArchives());
        }

        [Test]
        public void TestArchiveDiscoveryAndAssociationIgnoreCase() {
            var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            var dataPath = Path.Combine(root, "Data");
            Directory.CreateDirectory(dataPath);
            var archivePath = Path.Combine(dataPath, "Example - Main.BA2");
            File.WriteAllBytes(archivePath, Array.Empty<byte>());
            try {
                var session = new Session(Games.FO4, new SessionOptions {
                    gamePath = root,
                    loadResources = true
                });
                CollectionAssert.AreEqual(
                    new[] { archivePath },
                    session.resourceManager.GetAssociatedArchives("example.esp")
                );
            } finally {
                Directory.Delete(root, true);
            }
        }

        [Test]
        public void TestAutomaticStringTableKeyOmitsPluginExtension() {
            Assert.AreEqual(
                "skyrim_english",
                ResourceManager.GetStringTableKey("SkYrIm.EsM", "ENGLISH")
            );
        }

        [Test]
        public void TestMissingConfiguredDataFolderDoesNotThrow() {
            var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            try {
                Directory.CreateDirectory(root);
                var session = new Session(Games.SSE, new SessionOptions {
                    gamePath = root,
                    loadResources = true
                });
                Assert.IsEmpty(
                    session.resourceManager.GetAssociatedArchives("Skyrim.esm")
                );
                StringAssert.Contains(
                    "Data path not found",
                    string.Join("\n", session.logger.messages)
                );
            } finally {
                Directory.Delete(root, true);
            }
        }
    }

    public class RegHelperTests {
        [Test]
        public void TestTryGetRegKeyReturnsNullOutsideWindows() {
            if (OperatingSystem.IsWindows()) Assert.Ignore("Non-Windows behavior");
            Assert.IsNull(Games.SSE.GetInstallLocation());
        }
    }
}
