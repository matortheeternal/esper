using esper.elements;
using System.Collections.Generic;

namespace esper.conflicts {
    public class ConflictView {
        public ConflictRow row;

        public ConflictView(MainRecord rec) {
            var master = rec.master;
            var records = new List<Element>() { master };
            records.AddRange(master.overrides);
            row = new ConflictRow(records);
        }
    }
}
