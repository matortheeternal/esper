using esper;
using esper.conflicts;
using esper.defs;
using esper.elements;
using esper.plugins;
using esper.setup;
using NUnit.Framework;
using System.Linq;

namespace Tests.conflicts {
    // Single-plugin conflict views (OnlyOne branch), the Ignore
    // inversion guard, and the ChangesToJson stubs.
    public class ConflictViewEdgeTests {
        private Session session;
        private PluginFile plugin;
        private MainRecord rec;

        [OneTimeSetUp]
        public void SetUp() {
            session = new Session(Games.TES5, new SessionOptions());
            plugin = session.pluginManager.LoadPlugin(
                TestHelpers.FixturePath("ConflictTest.esp")
            );
            rec = (MainRecord)plugin.GetRecordByFormId(0x800);
        }

        [Test]
        public void TestSingleCellViewUsesMasterCellAndOnlyOneRow() {
            // A lone record is still the master cell; its leaf rows carry
            // the aggregate OnlyOne status.
            var view = new ConflictView(rec);
            var row = view.row;
            Assert.AreEqual(1, row.cells.Count);
            Assert.AreEqual(
                RowConflictStatus.OnlyOne, row.conflictStatus
            );
            Assert.AreEqual(
                CellConflictStatus.Master, row.cells[0].conflictStatus
            );
        }

        [Test]
        public void TestConflictCellNullValue() {
            var cell = new ConflictCell(null);
            Assert.AreEqual("", cell.value);
            Assert.IsFalse(cell.ignored);
            Assert.IsFalse(cell.isItm);
        }

        [Test]
        public void TestConflictCellIgnoredMatchesIgnoreStatus() {
            var view = new ConflictView(rec);
            var cell = view.row.cells[0];
            // ignored must agree with the calculator's own Ignore checks
            Assert.AreEqual(
                cell.conflictStatus == CellConflictStatus.Ignored,
                cell.ignored
            );
        }

        [Test]
        public void TestConflictCellIsItm() {
            var itmCell = new ConflictCell(null) {
                conflictStatus =
                    CellConflictStatus.IdenticalToMasterWinsConflict
            };
            Assert.IsTrue(itmCell.isItm);
            var normalCell = new ConflictCell(null) {
                conflictStatus = CellConflictStatus.ConflictWins
            };
            Assert.IsFalse(normalCell.isItm);
        }

        [Test]
        public void TestChangesToJsonReturnsNullWhenNoChanges() {
            var view = new ConflictView(rec);
            Assert.IsNull(view.ChangesToJson(rec));
            Assert.IsNull(view.row.ChangesToJson(0));
        }

        [Test]
        public void TestRowNameAndFirstElement() {
            var view = new ConflictView(rec);
            var edidRow = view.row.childRows[1];
            Assert.AreEqual("Editor ID", edidRow.name);
            Assert.IsNotNull(edidRow.firstElement);
        }

        [Test]
        public void TestRootRowInheritsMaxOfChildren() {
            var view = new ConflictView(rec);
            var root = view.row;
            var max = root.childRows
                .Select(r => (int)r.conflictStatus)
                .Max();
            Assert.AreEqual((RowConflictStatus)max, root.conflictStatus);
        }
    }
}
