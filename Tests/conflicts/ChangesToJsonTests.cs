using esper;
using esper.conflicts;
using esper.elements;
using esper.plugins;
using esper.setup;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Linq;

namespace Tests.conflicts {
    [TestFixture]
    public class ChangesToJsonTests {
        private Session session;
        private MainRecord master;
        private MainRecord over;

        [OneTimeSetUp]
        public void SetUp() {
            session = new Session(Games.TES5, new SessionOptions());
            foreach (var name in new[] {
                "ConflictTest.esp",
                "ConflictTest2.esp",
                "ConflictTest3.esp"
            })
                session.pluginManager.LoadPlugin(
                    TestHelpers.FixturePath(name)
                );
            master = session.pluginManager.plugins[0]
                .GetRecordByFormId(0x800);
            over = session.pluginManager.plugins[1]
                .GetRecordByFormId(0x800);
        }

        private static JObject ChangesFor(MainRecord target) {
            return new ConflictView(target).ChangesToJson(target) as JObject;
        }

        [Test]
        public void OverrideEnvelopeHasFormIdSigAndChanges() {
            var result = ChangesFor(over);
            Assert.IsNotNull(result);
            Assert.AreEqual("00000800", (string)result["formId"]);
            Assert.AreEqual("ARMO", (string)result["sig"]);
            var changes = result["changes"] as JArray;
            Assert.IsNotNull(changes);
            Assert.Greater(changes.Count, 0);
        }

        [Test]
        public void MasterViewReturnsNull() {
            Assert.IsNull(ChangesFor(master));
        }

        [Test]
        public void ChangeShapeInvariantsHold() {
            var changes = ChangesFor(over)["changes"] as JArray;
            foreach (JObject change in changes) {
                Assert.IsNotNull(change["path"]);
                Assert.IsNotNull(change["sortPath"]);
                var type = (int)change["type"];
                Assert.GreaterOrEqual(type, 0);
                Assert.LessOrEqual(type, 2);
                if (type == (int)ChangeType.Changed)
                    Assert.IsNotNull(change["value"]);
                else
                    Assert.IsNull(change["value"]);
            }
        }

        [Test]
        public void NoRecordHeaderPaths() {
            var changes = ChangesFor(over)["changes"] as JArray;
            foreach (JObject change in changes)
                Assert.IsFalse(
                    ((string)change["path"]).StartsWith("Record Header")
                );
        }

        [Test]
        public void SortPathUsesSortKeyAtArrayLevels() {
            var changes = ChangesFor(over)["changes"] as JArray;
            var kwda = changes.OfType<JObject>().FirstOrDefault(
                change => (string)change["sortPath"] == "KWDA\\00000000"
            );
            Assert.IsNotNull(kwda);
            Assert.AreEqual((int)ChangeType.Created, (int)kwda["type"]);
            Assert.IsNull(kwda["value"]);
        }
    }
}
