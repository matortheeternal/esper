using esper.elements;

namespace esper.conflicts {
    public class ConflictView {
        public ConflictRow row;

        public ConflictView(MainRecord rec) {
            var master = rec.master;
            var records = master.overrides;
            records.Insert(0, master);
            row = new ConflictRow(records);
        }
    }
}
