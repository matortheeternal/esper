using esper.elements;

namespace esper.conflicts {
    public class ConflictView {
        public ConflictRow row;

        public ConflictView(MainRecord rec) {
            var master = rec.master;
            var records = new List<Element>() { master };
            records.AddRange(master.overrides);
            row = new ConflictRow(records);
        }

        public JToken ChangesToJson(MainRecord rec) {
            var targetIndex = row.cells.FindIndex(cell => cell.element == rec);
            return row.ChangesToJson(targetIndex);
        }
    }
}
