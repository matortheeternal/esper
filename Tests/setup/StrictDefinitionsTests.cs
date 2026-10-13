using esper;
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
        public void TestLooseModeSkipsWithOneWarning() {
            var session = NewSession(false);
            Assert.IsNull(session.definitionManager.BuildDef(BogusDef()));
            Assert.IsNull(session.definitionManager.BuildDef(BogusDef()));
            var warnings = session.logger.messages
                .Where(m => m.messageType == LogMessageType.Warning)
                .Where(m => m.text.Contains("bogusDefType"))
                .ToList();
            Assert.AreEqual(1, warnings.Count);
        }

        [Test]
        public void TestStrictSessionBuildsCleanGameDefs() {
            // Must not throw: SSE definitions are fully implemented.
            var session = NewSession(true);
            Assert.IsNotNull(session.definitionManager);
        }
    }
}
