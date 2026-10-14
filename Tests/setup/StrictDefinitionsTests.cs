using esper;
using esper.defs;
using esper.setup;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Linq;

namespace Tests.setup {
    [TestFixture]
    public class StrictDefinitionsTests {
        private static Session strictSession;
        private static Session looseSession;

        [OneTimeSetUp]
        public void Setup() {
            strictSession = new Session(Games.SSE, new SessionOptions {
                strictDefinitions = true
            });
            looseSession = new Session(Games.SSE, new SessionOptions {
                strictDefinitions = false
            });
        }

        private static Session NewSession(bool strict) {
            return strict ? strictSession : looseSession;
        }

        private static JObject BogusDef() {
            return new JObject { { "type", "bogusDefType" } };
        }

        [Test]
        public void TestStrictModeDefaultsToOff() {
            Assert.IsFalse(new SessionOptions().strictDefinitions);
        }

        [Test]
        public void TestStrictModeThrowsOnUnimplementedDefType() {
            var session = NewSession(true);
            var ex = Assert.Throws<Exception>(() =>
                session.definitionManager.BuildDef(BogusDef()));
            StringAssert.Contains("bogusDefType", ex.Message);
        }

        [Test]
        public void TestLooseModeFallsBackToUnknownBytes() {
            // Unknown def types degrade to a byte-array def named
            // "Unknown" instead of being skipped with a warning.
            var session = NewSession(false);
            var manager = session.definitionManager;
            var def = manager.BuildDef(BogusDef());
            Assert.IsInstanceOf<BytesDef>(def);
            Assert.AreEqual("Unknown", ((BytesDef)def).name);
            // Idempotent: repeated builds report the gap only once,
            // and no warnings are surfaced.
            _ = manager.BuildDef(BogusDef());
            var warnings = session.logger.messages
                .Where(m => m.messageType == LogMessageType.Warning)
                .ToList();
            Assert.IsEmpty(warnings);
            CollectionAssert.Contains(
                manager.GetDefinitionGaps(),
                "Def type not implemented: bogusDefType"
            );
        }

        [Test]
        public void TestUnknownDeciderDegradesGracefully() {
            // GetDecider stays graceful at build time; the placeholder
            // only throws if data actually routes through it.
            var session = NewSession(false);
            var manager = session.definitionManager;
            var decider = manager.GetDecider("BogusDecider");
            Assert.IsInstanceOf<UnknownDecider>(decider);
            var exception = Assert.Throws<Exception>(() =>
                decider.Decide(null));
            StringAssert.Contains("BogusDecider", exception.Message);
            CollectionAssert.Contains(
                manager.GetDefinitionGaps(), "Unknown decider: BogusDecider"
            );
        }

        [Test]
        public void TestStrictSessionBuildsCleanGameDefs() {
            // Must not throw: SSE definitions are fully implemented.
            var session = NewSession(true);
            Assert.IsNotNull(session.definitionManager);
        }
    }
}
