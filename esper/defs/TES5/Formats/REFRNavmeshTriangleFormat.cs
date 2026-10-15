using esper.data;
using esper.elements;
using esper.resolution;
using esper.setup;
using esper.warnings;

namespace esper.defs.TES5 {
    public class REFRNavmeshTriangleFormat : FormatDef {
        public static readonly string defId = "REFRNavmeshTriangleFormat";

        internal REFRNavmeshTriangleFormat(DefinitionManager manager, JObject src)
            : base(manager, src) { }

        private MainRecord ResolveNavmesh(ValueElement element) {
            return element?.container?.elements?[0].referencedRecord;
        }

        public void GetWarnings(
            ValueElement element, List<ElementWarning> warnings
        ) {
            int index = (int)element.data.ToInt64();
            var rec = ResolveNavmesh(element);
            if (rec == null) return;
            if (rec.signature != Signatures.NAVM) {
                warnings.Add(new ElementWarning(element) {
                    warning = $"\"{rec.name}\" is not a Navmesh record"
                });
                return;
            }
            Container triangles = (Container)rec.GetElement(@"NVNM\Triangles");
            if (triangles == null || triangles.elements.Count > index) return;
            warnings.Add(new ElementWarning(element) {
                warning = $"Navmesh triangle not found in \"{rec.name}\""
            });
        }

        internal override string DataToValue(ValueElement element, VariantData data) {
            return data.ToDisplayString();
        }

        internal override VariantData ValueToData(ValueElement element, string value) {
            return Int64.Parse(value);
        }
    }
}
