using esper.defs;
using esper.elements;
using System.Collections.Generic;
using System.Linq;

namespace esper.conflicts {
    public class ConflictRow {
        public RowConflictStatus conflictStatus;
        public List<ConflictRow> childRows;
        public List<ConflictCell> cells;

        public ConflictRow(List<Element> elements) {
            InitCells(elements);
            LoadChildRows(elements);
            CalculateConflicts(elements);
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

        private void AppendUnsortedRow(
            int elementCount, List<int> counts, 
            List<Container> containers, int elementIndex
        ) {
            var childElements = new List<Element>(elementCount);
            for (int i = 0; i < elementCount; i++) {
                childElements.Add(elementIndex < counts[i]
                    ? containers[i].elements[elementIndex]
                    : null);
            }
            childRows.Add(new ConflictRow(childElements));
        }

        private void AddUnsortedChildRows(List<Element> elements) {
            var elementCount = elements.Count;
            var containers = elements.Select(e => e as Container).ToList();
            var counts = containers.Select(c => c != null ? c.count : 0).ToList();
            for (int i = 0; i < counts.Max(); i++)
                AppendUnsortedRow(elementCount, counts, containers, i);
        }

        private List<Element> MakeElementList(int numCells) {
            var list = new List<Element>(numCells);
            for (int i = 0; i < numCells; i++) list.Add(null);
            return list;
        }

        private void SortElement(
            Element element, SortedDictionary<string, List<Element>> rows, 
            int numCells, int index
        ) {
            var sortKey = element.sortKey;
            if (!rows.ContainsKey(sortKey))
                rows[sortKey] = MakeElementList(numCells);
            rows[sortKey][index] = element;
        }

        private void AddSortedChildRows(List<Element> elements) {
            var containers = elements.Select(e => e as Container).ToList();
            var rows = new SortedDictionary<string, List<Element>>();
            var numCells = containers.Count;
            for (int i = 0; i < numCells; i++)
                foreach (var element in containers[i].elements)
                    SortElement(element, rows, numCells, i);
            foreach (var entry in rows)
                childRows.Add(new ConflictRow(entry.Value));
        }

        private void LoadChildRows(List<Element> elements) {
            var firstElement = elements.First(e => e != null);
            if (!(firstElement is Container container)) return;
            childRows = new List<ConflictRow>();
            if (firstElement.def is ArrayDef aDef && aDef.sorted) {
                AddSortedChildRows(elements);
            } else {
                AddUnsortedChildRows(elements);
            }
        }
    }
}
