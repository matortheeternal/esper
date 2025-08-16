using esper.elements;

namespace esper.helpers {
    public static class ElementHelpers {
        public static string StructSortKey(
            Element element, List<int> sortKeyIndices
        ) {
            if (sortKeyIndices == null) return "";
            var container = (Container)element;
            return string.Join("|", sortKeyIndices.Select(index => {
                return container.elements[index].sortKey;
            }).ToArray());
        }
    }
}
