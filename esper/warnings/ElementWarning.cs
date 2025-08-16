using esper.elements;

namespace esper.warnings {
    [JSExport]
    public class ElementWarning {
        public Element element;
        public string warning = "";

        public ElementWarning(Element element) {
            this.element = element;
        }
    }
}
