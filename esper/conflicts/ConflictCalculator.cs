using esper.defs;
using esper.elements;
using System.Collections.Generic;
using System.Linq;

namespace esper.conflicts {
    internal class ConflictCalculator {
        private readonly List<Element> elements;
        private readonly ConflictRow row;
        private readonly Element firstElement;
        private ConflictType? _conflictType;
        private bool? _hasConflict;
        private List<string> _cellValues;
        private HashSet<string> _uniqueValues;
        private bool? _overrideConflict;

        private ElementDef firstDef => firstElement?.def;
        private bool firstElementIgnored => 
            firstDef?.conflictType == ConflictType.Ignore;
        private int cellCount => row.cells.Count;
        private int numEmptyValues => UniqueValues.Contains("") ? 1 : 0;

        private ConflictType ComputeOverallConflictType() {
            if (firstElement == null) return ConflictType.Normal;
            if (!firstElement.def.dynamicConflictType)
                return firstElement.def.conflictType;
            return (ConflictType)elements.Select(e => e?.def.conflictType).Max();
        }

        private ConflictType conflictType {
            get {
                if (_conflictType == null)
                    _conflictType = ComputeOverallConflictType();
                return (ConflictType) _conflictType;
            }
        }

        private List<string> CellValues {
            get {
                if (_cellValues == null)
                    _cellValues = row.cells.Select(cell => cell.value).ToList();
                return _cellValues;
            }
        }

        private string firstCellValue => CellValues.First();
        private string lastCellValue => CellValues.Last();

        private HashSet<string> GetUniqueValues() {
            return CellValues.ToHashSet();
        }

        private HashSet<string> UniqueValues {
            get {
                if (_uniqueValues == null)
                    _uniqueValues = GetUniqueValues();
                return _uniqueValues;
            }
        }

        private bool GetHasConflict() {
            return UniqueValues.Count > 1;
        }

        private bool hasConflict {
            get {
                if (_hasConflict == null)
                    _hasConflict = GetHasConflict();
                return (bool) _hasConflict;
            }
        }

        private bool ComputeOverrideConflict() {
            if (_uniqueValues.Count != 2) return false;
            var e = elements.First();
            var compare = elements.Last();
            var nonMatchingValues = (e == null) != (compare == null) ||
                (e != null && firstCellValue != lastCellValue);
            var valueOverride = UniqueValues.Contains("") && 
                compare != null && lastCellValue != "";
            return nonMatchingValues || valueOverride;
        }

        private bool overrideConflict {
            get {
                if (_overrideConflict == null)
                    _overrideConflict = ComputeOverrideConflict();
                return (bool)_overrideConflict;
            }
        }

        internal ConflictCalculator(List<Element> elements, ConflictRow row) {
            this.elements = elements;
            this.row = row;
            firstElement = elements.First(e => e != null);
        }

        private bool ShouldIgnore(ConflictCell cell) {
            return conflictType == ConflictType.Ignore ||
                conflictType == ConflictType.NormalIgnoreEmpty &&
                cell.element == null;
        }

        private bool IsITM(string cellValue) {
            return cellValue == firstCellValue &&
               (conflictType <= ConflictType.Ignore || !firstElementIgnored);
        }

        internal CellConflictStatus CalculateCellConflict(
            ConflictCell cell, int index
        ) {
            if (firstElement == null) return CellConflictStatus.NotDefined;
            if (ShouldIgnore(cell)) return CellConflictStatus.Ignored;
            if (cellCount == 1) return CellConflictStatus.OnlyOne;
            if (index == 0) return CellConflictStatus.Master;
            if (hasConflict && conflictType == ConflictType.Benign)
                return CellConflictStatus.ConflictBenign;
            var cellValue = CellValues[index];
            if (IsITM(cellValue))
                return (index == cellCount - 1 && hasConflict)
                    ? CellConflictStatus.IdenticalToMasterWinsConflict
                    : CellConflictStatus.IdenticalToMaster;
            if (cellValue == lastCellValue && overrideConflict)
                return CellConflictStatus.Override;
            if (hasConflict && cell.element != firstElement && !cell.ignored)
                return cellValue == lastCellValue
                    ? CellConflictStatus.ConflictWins
                    : CellConflictStatus.ConflictLoses;
            if (firstDef?.defType == XEDefType.dtRecord)
                return CellConflictStatus.HiddenByModGroup;
            return CellConflictStatus.Unknown;
        }

        internal RowConflictStatus CalculateRowConflict() {
            if (cellCount == 0) return RowConflictStatus.Unknown;
            if (cellCount == 1) return RowConflictStatus.OnlyOne;
            var valueCount = UniqueValues.Count;
            if (valueCount == 1) return RowConflictStatus.NoConflict;
            if (conflictType == ConflictType.Benign)
                return RowConflictStatus.ConflictBenign;
            if (overrideConflict)
                return RowConflictStatus.Override;
            if (conflictType == ConflictType.Critical &&
                valueCount - numEmptyValues > 1) 
                return RowConflictStatus.ConflictCritical;
            return RowConflictStatus.Conflict;
        }
    }
}
