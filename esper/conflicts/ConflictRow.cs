using esper.defs;
using esper.elements;

namespace esper.conflicts {
    [JSExport]
    public class ConflictRow {
        public RowConflictStatus conflictStatus;
        public List<ConflictRow> childRows;
        public List<ConflictCell> cells;
        public Element firstElement => cells.First(cell => cell.element != null).element;
        public string name => firstElement.name;

        public ConflictRow(List<Element> elements) {
            InitCells(elements);
            LoadChildRows(elements);
            if (childRows != null) {
                InheritConflictStatus();
            } else {
                CalculateConflicts(elements);
            }
        }

        private CellConflictStatus InheritCellConflictStatus(int index) {
            return childRows.Select(row => row.cells[index].conflictStatus).Max();
        }

        private void InheritConflictStatus() {
            conflictStatus = childRows.Select(r => r.conflictStatus).Max();
            for (int i = 0; i < cells.Count; i++) {
                var cell = cells[i];
                cell.conflictStatus = InheritCellConflictStatus(i);
            }
        }

        private void CalculateConflicts(List<Element> elements) {
            var calc = new ConflictCalculator(elements, this);
            for (var i = 0; i < cells.Count; i++) {
                var cell = cells[i];
                cell.conflictStatus = calc.CalculateCellConflict(cell, i);
            }
            conflictStatus = calc.CalculateRowConflict();
        }

        private void InitCells(List<Element> elements) {
            cells = new List<ConflictCell>(elements.Count);
            foreach (Element element in elements) {
                var cell = new ConflictCell(element);
                cells.Add(cell);
            }
        }

        private void AssignChildren(
            SortedDictionary<int, Element[]> entries, 
            int elementCount, Container container, int columnIndex
        ) {
            var childDefs = container.def.childDefs;
            foreach (var child in container.elements) {
                var rowIndex = childDefs.IndexOf(child.def);
                if (!entries.ContainsKey(rowIndex))
                    entries[rowIndex] = new Element[elementCount];
                entries[rowIndex][columnIndex] = child;
            }
        }

        private void AddUnsortedChildRows(List<Element> elements) {
            var elementCount = elements.Count;
            var entries = new SortedDictionary<int, Element[]>();
            for (int i = 0; i < elementCount; i++) {
                var container = elements[i] as Container;
                AssignChildren(entries, elementCount, container, i);
            }
            foreach (var entry in entries)
                childRows.Add(new ConflictRow(entry.Value.ToList()));
        }

        private List<Element> MakeElementList(int numCells) {
            var list = new List<Element>(numCells);
            for (int i = 0; i < numCells; i++) list.Add(null);
            return list;
        }

        private void SortElement(
            Element element, SortedDictionary<string, List<Element>> rows, 
            int numCells, int index, int repeat = 1
        ) {
            var sortKey = element.sortKey;
            if (repeat > 1) sortKey = $"{sortKey}-{repeat}";
            if (!rows.ContainsKey(sortKey))
                rows[sortKey] = MakeElementList(numCells);
            if (rows[sortKey][index] != null) {
                SortElement(element, rows, numCells, index, repeat + 1);
                return;
            }
            rows[sortKey][index] = element;
        }

        private void AddSortedChildRows(List<Element> elements) {
            var containers = elements.Select(e => e as Container).ToList();
            var rows = new SortedDictionary<string, List<Element>>();
            var numCells = containers.Count;
            for (int i = 0; i < numCells; i++) {
                if (containers[i] == null) continue;
                foreach (var element in containers[i].elements)
                    SortElement(element, rows, numCells, i);
            }
            foreach (var entry in rows)
                childRows.Add(new ConflictRow(entry.Value));
        }

        private void LoadChildRows(List<Element> elements) {
            var firstElement = elements.First(e => e != null);
            if (!(firstElement is Container)) return;
            childRows = new List<ConflictRow>();
            if (firstElement.def is ArrayDef aDef && aDef.sorted) {
                AddSortedChildRows(elements);
            } else {
                AddUnsortedChildRows(elements);
            }
        }

        public JToken ChangesToJson(int index) {
            var changes = new JObject();
            var cell = cells[index];
            if (cell.isItm) return null;
            foreach (var childRow in childRows) {
                // TODO
            }
            return changes;
        }
    }
}
