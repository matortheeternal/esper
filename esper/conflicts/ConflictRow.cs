using esper.elements;
using System.Collections.Generic;

namespace esper.conflicts {
    public class ConflictRow {
        public RowConflictStatus conflictStatus;
        public List<ConflictRow> childRows;
        public List<ConflictCell> cells;

        public ConflictRow(List<Element> elements) {
            cells = new List<ConflictCell>(elements.Count);
            childRows = new List<ConflictRow>(elements.Count);
            InitCells(elements);
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
            foreach (Element element in elements) {
                var cell = new ConflictCell(element);
                cells.Add(cell);
            }
        }
    }
}
