using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using esper.conflicts;

namespace Tests.conflicts {
    public static class Helpers {
        public static void ExportConflictRow(JArray output, ConflictRow row) {
            var cellConflicts = row.cells.Select(cell => cell.conflictStatus.ToString());
            var obj = new JObject {
                { "label", row.name },
                { "rowConflict", row.conflictStatus.ToString() },
                { "cellConflicts", new JArray(cellConflicts) }
            };
            output.Add(obj);
            if (row.childRows == null) return;
            row.childRows.ForEach(childRow => ExportConflictRow(output, childRow));
        }

        public static void ExportConflicts(ConflictView view, string filename) {
            var output = new JArray();
            ExportConflictRow(output, view.row);
            var outputPath = Path.Join(Environment.CurrentDirectory, filename);
            File.WriteAllText(outputPath, output.ToString());
        }
    }
}
