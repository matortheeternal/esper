using esper.elements;

namespace esper.conflicts {
    [JSExport]
    public class ConflictView {
        public ConflictRow row;

        public ConflictView(MainRecord rec) {
            var master = rec.master;
            var records = new List<Element>() { master };
            records.AddRange(master.overrides);
            row = new ConflictRow(records);
        }

        internal JToken ChangesToJson(MainRecord rec) {
            var targetIndex = row.cells.FindIndex(cell => cell.element == rec);
            if (targetIndex <= 0) return null;
            var changes = row.ChangesToJson(targetIndex) as JArray;
            if (changes == null || changes.Count == 0) return null;
            var result = new JObject();
            result["formId"] = rec.master.fileFormId.ToString("X8");
            result["sig"] = rec.signature.ToString();
            result["changes"] = changes;
            return result;
        }
    }
}
