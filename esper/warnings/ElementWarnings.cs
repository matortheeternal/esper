using esper.elements;

namespace esper.warnings {
    [JSExport]
    public class ElementWarnings : List<ElementWarning> {
        public bool Add(Element element, string warning) {
            Add(new ElementWarning(element) {
                warning = warning
            });
            return true;
        }
    }
}
