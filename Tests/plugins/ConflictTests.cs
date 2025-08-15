using esper;
using esper.conflicts;
using esper.plugins;
using esper.setup;
using NUnit.Framework;

namespace Tests.plugins {
    public class ConflictTests {
        public Session session;
        public PluginManager pluginManager => session.pluginManager;
        private ConflictView view1;
        private ConflictView view2;
        private ConflictView view3;

        private void LoadPlugins(string[] filenames) {
            foreach (var filename in filenames) {
                var pluginPath = TestHelpers.FixturePath(filename);
                pluginManager.LoadPlugin(pluginPath);
            }
        }

        [OneTimeSetUp]
        public void SetUp() {
            session = new Session(Games.TES5, new SessionOptions());
            LoadPlugins(new string[] { 
                "ConflictTest.esp", 
                "ConflictTest2.esp", 
                "ConflictTest3.esp" 
            });                                                                                         
            Assert.AreEqual(pluginManager.plugins.Count, 3);
            Assert.AreEqual(pluginManager.plugins[0].name, "ConflictTest.esp");
            Assert.AreEqual(pluginManager.plugins[1].name, "ConflictTest2.esp");
            Assert.AreEqual(pluginManager.plugins[2].name, "ConflictTest3.esp");
            var plugin1 = pluginManager.plugins[0] as IRecordManager;
            view1 = new ConflictView(plugin1.GetRecordByFormId(0x800));
            view2 = new ConflictView(plugin1.GetRecordByFormId(0x801));
            view3 = new ConflictView(plugin1.GetRecordByFormId(0x802));
        }

        private void TestConflictStates(
            ConflictRow row, 
            CellConflictStatus[] cellConflictStates, 
            RowConflictStatus rowConfictStatus
        ) {
            for (int i = 0; i < cellConflictStates.Length; i++) {
                var cell = row.cells[i];
                Assert.AreEqual(cellConflictStates[i], cell.conflictStatus);
            }
            Assert.AreEqual(rowConfictStatus, row.conflictStatus);
        }

        [Test]
        public void TestRecordHeaderRow() {
            var recordHeaderRow = view1.row.childRows[0];
            TestConflictStates(recordHeaderRow, new CellConflictStatus[] {
                CellConflictStatus.Master,
                CellConflictStatus.IdenticalToMaster,
                CellConflictStatus.IdenticalToMaster
            }, RowConflictStatus.NoConflict);
        }

        [Test]
        public void TestEditorIdRow() {
            var edidRow = view1.row.childRows[1];
            TestConflictStates(edidRow, new CellConflictStatus[] {
                CellConflictStatus.Master,
                CellConflictStatus.IdenticalToMaster,
                CellConflictStatus.IdenticalToMaster
            }, RowConflictStatus.NoConflict);
        }

        [Test]
        public void TestObjectBounds() {
            var obndRow = view1.row.childRows[2];
            var cellConflictStates = new CellConflictStatus[] {
                CellConflictStatus.Master,
                CellConflictStatus.ConflictLoses,
                CellConflictStatus.IdenticalToMasterWinsConflict
            };
            TestConflictStates(obndRow, cellConflictStates, RowConflictStatus.Conflict);
            var x1Row = obndRow.childRows[0];
            TestConflictStates(x1Row, cellConflictStates, RowConflictStatus.Conflict);
            var y1Row = obndRow.childRows[1];
            TestConflictStates(y1Row, cellConflictStates, RowConflictStatus.Conflict);
            var z1Row = obndRow.childRows[2];
            TestConflictStates(z1Row, new CellConflictStatus[] {
                CellConflictStatus.Master,
                CellConflictStatus.IdenticalToMaster,
                CellConflictStatus.Override
            }, RowConflictStatus.Override);
        }

        [Test]
        public void TestFullNameRow() {
            var fullRow = view1.row.childRows[3];
            TestConflictStates(fullRow, new CellConflictStatus[] {
                CellConflictStatus.NotDefined,
                CellConflictStatus.NotDefined,
                CellConflictStatus.Override
            }, RowConflictStatus.Override);
        }

        [Test]
        public void TestKsizRow() {
            var fullRow = view1.row.childRows[4];
            TestConflictStates(fullRow, new CellConflictStatus[] {
                CellConflictStatus.NotDefined,
                CellConflictStatus.ConflictBenign,
                CellConflictStatus.ConflictBenign
            }, RowConflictStatus.ConflictBenign);
        }

        [Test]
        public void TestKeywords() {
            var kwdaRow = view1.row.childRows[5];
            TestConflictStates(kwdaRow, new CellConflictStatus[] {
                CellConflictStatus.NotDefined,
                CellConflictStatus.Override,
                CellConflictStatus.Override
            }, RowConflictStatus.Override);
            TestConflictStates(kwdaRow.childRows[0], new CellConflictStatus[] {
                CellConflictStatus.NotDefined,
                CellConflictStatus.Override,
                CellConflictStatus.Override
            }, RowConflictStatus.Override);
            TestConflictStates(kwdaRow.childRows[1], new CellConflictStatus[] {
                CellConflictStatus.NotDefined,
                CellConflictStatus.NotDefined,
                CellConflictStatus.Override
            }, RowConflictStatus.Override);
        }

        [Test]
        public void TestData() {
            var dataRow = view1.row.childRows[6];
            TestConflictStates(dataRow, new CellConflictStatus[] {
                CellConflictStatus.Master,
                CellConflictStatus.ConflictLoses,
                CellConflictStatus.ConflictWins
            }, RowConflictStatus.Conflict);
            var valueRow = dataRow.childRows[0];
            TestConflictStates(valueRow, new CellConflictStatus[] {
                CellConflictStatus.Master,
                CellConflictStatus.ConflictLoses,
                CellConflictStatus.ConflictWins
            }, RowConflictStatus.Conflict);
            var weightRow = dataRow.childRows[1];
            TestConflictStates(weightRow, new CellConflictStatus[] {
                CellConflictStatus.Master,
                CellConflictStatus.Override,
                CellConflictStatus.Override
            }, RowConflictStatus.Override);
        }

        [Test]
        public void TestDnam() {
            var dnamRow = view1.row.childRows[7];
            TestConflictStates(dnamRow, new CellConflictStatus[] {
                CellConflictStatus.Master,
                CellConflictStatus.ConflictLoses,
                CellConflictStatus.IdenticalToMasterWinsConflict
            }, RowConflictStatus.Conflict);
        }

        [Test]
        public void TestTnam() {
            var tnamRow = view1.row.childRows[8];
            TestConflictStates(tnamRow, new CellConflictStatus[] {
                CellConflictStatus.Master,
                CellConflictStatus.ConflictLoses,
                CellConflictStatus.NotDefined
            }, RowConflictStatus.Conflict);
        }
    }
}
