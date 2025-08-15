using esper.elements;

namespace esper.conflicts {
    public class ConflictCell {
        public Element element;
        public CellConflictStatus conflictStatus;

        public string value => element != null ? element.sortKey : "";
        public bool ignored => element?.def.conflictType != defs.ConflictType.Ignore;
        public bool isItm => conflictStatus == CellConflictStatus.IdenticalToMaster || 
            conflictStatus == CellConflictStatus.IdenticalToMasterWinsConflict;

        public ConflictCell(Element element) {
            this.element = element;
        }
    }
}
