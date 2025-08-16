namespace esper.elements {
    [JSExport]
    public class AssignmentInfo {
        public int index = 0;
        public bool assigned = false;

        internal void Assign(Container container, Element element, bool replace) {
            if (replace && assigned) {
                container.internalElements[index] = element;
            } else {
                container.internalElements.Insert(index, element);
            }
        }
    }
}
