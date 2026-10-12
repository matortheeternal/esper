using esper;
using esper.helpers;
using esper.setup;
using NUnit.Framework;
using System;
using System.IO;
using Tests;

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
    }

    public class SessionLifecycleTests {
        [Test]
        public void TestSessionLogsStartupMessages() {
            var session = new Session(Games.SSE, new SessionOptions());
            var messages = string.Join("\n", session.logger.messages);
            StringAssert.Contains("Esper v", messages);
            StringAssert.Contains(
                "Starting a new session for Skyrim SE", messages
            );
        }

        [Test]
        public void TestMissingGameIniLogsError() {
            var session = new Session(Games.SSE, new SessionOptions());
            // no Skyrim.ini exists on this machine
            Assert.IsFalse(File.Exists(Games.SSE.iniPath));
            var messages = string.Join("\n", session.logger.messages);
            StringAssert.Contains("Game ini file not found", messages);
            Assert.IsNull(session.gameIni.GetConfiguredLanguage());
        }
    }

    public class PluginManagerLifecycleTests {
        private Session session;

        [SetUp]
        public void SetUp() {
            session = new Session(Games.SSE, new SessionOptions());
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
            session = new Session(Games.SSE, new SessionOptions());
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
            var session = new Session(Games.SSE, new SessionOptions());
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
            var session = new Session(Games.SSE, new SessionOptions());
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
            var session = new Session(Games.SSE, new SessionOptions());
            var file = new LoadOrderFile(session);
            Assert.IsFalse(File.Exists(file.filePath));
            Assert.IsEmpty(file.lines);
            Assert.IsTrue(file.usesAsterisks);
        }

        [Test]
        public void TestLoadOrderLineParsing() {
            var session = new Session(Games.SSE, new SessionOptions());
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
            var session = new Session(Games.TES5, new SessionOptions());
            var file = new LoadOrderFile(session);
            Assert.IsFalse(file.usesAsterisks);
            var line = new LoadOrderLine(file, "Skyrim.esm", 0);
            Assert.IsTrue(line.isActive);
            Assert.AreEqual("Skyrim.esm", line.moduleName);
        }
    }

    public class ResourceManagerTests {
        [Test]
        public void TestNoArchivesRegisteredByDefault() {
            var session = new Session(Games.SSE, new SessionOptions());
            Assert.IsEmpty(
                session.resourceManager.GetAssociatedArchives("Skyrim.esm")
            );
        }
    }

    public class RegHelperTests {
        [Test]
        public void TestTryGetRegKeyReturnsNullOnLinux() {
            // RegOpenKeyEx is a Windows P/Invoke; on Linux the call
            // throws and TryGetRegKey swallows it.
            Assert.IsNull(RegistryWOW6432.TryGetRegKey(
                RegHive.HKEY_LOCAL_MACHINE,
                @"SOFTWARE\Bethesda Softworks\Skyrim Special Edition",
                "Installed Path"
            ));
        }
    }
}
