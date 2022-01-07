using esper.elements;
using System.Collections.Generic;

namespace esper.conflicts {
    public class ConflictRow {
        public RowConflictStatus conflictStatus;
        public List<ConflictRow> childRows;
        public List<ConflictCell> childCells;

        public ConflictRow(List<Element> elements) {
            childCells = new List<ConflictCell>(elements.Count);
            childRows = new List<ConflictRow>(elements.Count);
            InitChildCells(elements);
            var calc = new ConflictCalculator(elements, this);
            calc.CalculateCellConflicts();
            conflictStatus = calc.CalculateRowConflict();
        }

        private void InitChildCells(List<Element> elements) {
            foreach (Element element in elements) {
                var cell = new ConflictCell(element);
                childCells.Add(cell);
            }
        }
    }
}
