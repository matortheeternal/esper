using esper;
using esper.elements;
using esper.resolution;
using esper.setup;
using NUnit.Framework;
using System.Linq;
using Tests;

namespace Tests.resolution {
    [TestFixture]
    public class SignatureResolutionTests {
        private PluginManager pluginManager;
        private dynamic plugin;

        [OneTimeSetUp]
        public void SetUp() {
            var session = new Session(Games.SSE, new SessionOptions());
            pluginManager = session.pluginManager;
            plugin = pluginManager.LoadPlugin(TestHelpers.FixturePath("AllRecords.esp"));
        }

        private MainRecord FirstActi() {
            Container root = plugin;
            var group = (GroupRecord)root.elements
                .First(e => e is GroupRecord g && g.groupDef.signature.ToString() == "ACTI");
            return (MainRecord)group.ResolveElement("ACTI");
        }

        [Test]
        public void MemberBySigEqualsMemberByName() {
            var rec = FirstActi();
            var bySig = rec.GetElement("OBND");
            var byName = rec.GetElement("Object Bounds");
            Assert.IsNotNull(bySig);
            Assert.AreSame(bySig, byName);
        }

        [Test]
        public void LongFormSigDashNameResolves() {
            var rec = FirstActi();
            Assert.AreSame(rec.GetElement("OBND"), rec.GetElement("OBND - Object Bounds"));
        }

        [Test]
        public void NestedRecordBySigWithinGroup() {
            Container root = plugin;
            var group = (GroupRecord)root.ResolveElement("ACTI");
            var rec = FirstActi();
            Assert.AreSame(rec, group.ResolveElement("ACTI"));
        }

        [Test]
        public void UnknownSigReturnsNull() {
            Assert.IsNull(FirstActi().GetElement("ZZZZ"));
        }

        [Test]
        public void LowercaseSigDoesNotMatch() {
            var rec = FirstActi();
            Assert.IsNull(rec.GetElement("obnd"));
            Assert.IsNotNull(rec.GetElement("OBND"));
        }
    }
}
