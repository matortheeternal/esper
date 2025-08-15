using esper;
using esper.conflicts;
using esper.plugins;
using esper.setup;
using NUnit.Framework;

namespace Tests.conflicts {
    public class TestArmo801 {
        public Session session;
        public PluginManager pluginManager => session.pluginManager;
        private ConflictView view1;

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
            view1 = new ConflictView(plugin1.GetRecordByFormId(0x801));
            Helpers.ExportConflicts(view1, "0x801.json");
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
        public void TestRecord() {
            var recordRow = view1.row;
            TestConflictStates(recordRow, new CellConflictStatus[] {
                CellConflictStatus.Master,
                CellConflictStatus.IdenticalToMaster
            }, RowConflictStatus.NoConflict);
        }

        [Test]
        public void TestRecordHeaderRow() {
            var recordHeaderRow = view1.row.childRows[0];
            TestConflictStates(recordHeaderRow, new CellConflictStatus[] {
                CellConflictStatus.Master,
                CellConflictStatus.IdenticalToMaster
            }, RowConflictStatus.NoConflict);
            var recordFlagsRow = recordHeaderRow.childRows[2];
            TestConflictStates(recordFlagsRow, new CellConflictStatus[] {
                CellConflictStatus.Master,
                CellConflictStatus.IdenticalToMaster
            }, RowConflictStatus.NoConflict);
            var versionControlInfo1Row = recordHeaderRow.childRows[4];
            TestConflictStates(versionControlInfo1Row, new CellConflictStatus[] {
                CellConflictStatus.Ignored,
                CellConflictStatus.Ignored
            }, RowConflictStatus.NoConflict);
        }

        [Test]
        public void TestEditorIdRow() {
            var edidRow = view1.row.childRows[1];
            TestConflictStates(edidRow, new CellConflictStatus[] {
                CellConflictStatus.Master,
                CellConflictStatus.IdenticalToMaster
            }, RowConflictStatus.NoConflict);
        }

        [Test]
        public void TestObjectBounds() {
            var obndRow = view1.row.childRows[2];
            var cellConflictStates = new CellConflictStatus[] {
                CellConflictStatus.Master,
                CellConflictStatus.IdenticalToMaster
            };
            TestConflictStates(obndRow, cellConflictStates, RowConflictStatus.NoConflict);
            var x1Row = obndRow.childRows[0];
            TestConflictStates(x1Row, cellConflictStates, RowConflictStatus.NoConflict);
            var y1Row = obndRow.childRows[1];
            TestConflictStates(y1Row, new CellConflictStatus[] {
                CellConflictStatus.Master,
                CellConflictStatus.IdenticalToMaster
            }, RowConflictStatus.NoConflict);
            var z1Row = obndRow.childRows[2];
            TestConflictStates(z1Row, new CellConflictStatus[] {
                CellConflictStatus.Master,
                CellConflictStatus.IdenticalToMaster
            }, RowConflictStatus.NoConflict);
        }

        [Test]
        public void TestData() {
            var dataRow = view1.row.childRows[3];
            TestConflictStates(dataRow, new CellConflictStatus[] {
                CellConflictStatus.Master,
                CellConflictStatus.IdenticalToMaster
            }, RowConflictStatus.NoConflict);
            var valueRow = dataRow.childRows[0];
            TestConflictStates(valueRow, new CellConflictStatus[] {
                CellConflictStatus.Master,
                CellConflictStatus.IdenticalToMaster
            }, RowConflictStatus.NoConflict);
            var weightRow = dataRow.childRows[1];
            TestConflictStates(weightRow, new CellConflictStatus[] {
                CellConflictStatus.Master,
                CellConflictStatus.IdenticalToMaster
            }, RowConflictStatus.NoConflict);
        }

        [Test]
        public void TestDnam() {
            var dnamRow = view1.row.childRows[4];
            TestConflictStates(dnamRow, new CellConflictStatus[] {
                CellConflictStatus.Master,
                CellConflictStatus.IdenticalToMaster
            }, RowConflictStatus.NoConflict);
        }
    }
}
