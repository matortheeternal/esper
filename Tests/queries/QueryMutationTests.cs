#nullable enable

using esper;
using esper.elements;
using esper.plugins;
using esper.queries;
using esper.setup;
using NUnit.Framework;
using System;
using System.Linq;

namespace Tests.queries {
    public class QueryMutationTests {
        private Session session = null!;
        private PluginFile plugin = null!;

        [SetUp]
        public void SetUp() {
            session = new Session(Games.SSE, new SessionOptions());
            plugin = session.pluginManager.LoadPlugin(
                TestHelpers.FixturePath("ARecords.esp")
            );
        }

        [Test]
        public void TestUpdateChangesSelectedRecords() {
            QueryResult result = Query.Parse(
                "UPDATE records SET editorId = 'UpdatedArmor' "
                + "WHERE editorId = 'TestArmor'"
            ).Run(session);

            Assert.That(result.elements, Has.Count.EqualTo(1));
            Assert.That(
                plugin.records.Single(
                    record => record.signature.ToString() == "ARMO"
                ).editorId,
                Is.EqualTo("UpdatedArmor")
            );
        }

        [Test]
        public void TestUpdateChangesSelectedValues() {
            QueryResult result = Query.Parse(
                "UPDATE values SET value = 'UpdatedArmor' "
                + "WHERE signature = EDID AND value = 'TestArmor'"
            ).Run(session);

            Assert.That(result.elements, Has.Count.EqualTo(1));
            Assert.That(
                plugin.records.Single(
                    record => record.signature.ToString() == "ARMO"
                ).editorId,
                Is.EqualTo("UpdatedArmor")
            );
        }

        [Test]
        public void TestUpdateValidatesEverySelectedElementBeforeWriting() {
            InvalidOperationException? error = Assert.Throws<InvalidOperationException>(
                () => Query.Parse(
                    "UPDATE records SET value = 'Changed' "
                    + "WHERE signature IN (AACT, ARMO)"
                ).Run(session)
            );

            Assert.That(error!.Message, Does.StartWith("UPDATE cannot set 'value'"));
            Assert.That(
                plugin.records.Select(record => record.editorId),
                Does.Contain("TestAction")
            );
            Assert.That(
                plugin.records.Select(record => record.editorId),
                Does.Contain("TestArmor")
            );
        }

        [Test]
        public void TestDeleteRemovesSelectedRecords() {
            QueryResult result = Query.Parse(
                "DELETE records WHERE editorId = 'TestArmor'"
            ).Run(session);

            Assert.That(result.elements, Has.Count.EqualTo(1));
            Assert.That(plugin.records, Has.Count.EqualTo(8));
            Assert.That(
                plugin.records.Select(record => record.editorId),
                Does.Not.Contain("TestArmor")
            );
        }
    }
}
