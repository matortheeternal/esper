using esper.elements;
using System.Collections.Generic;

namespace esper.helpers {
    public static class ElementHelpers {
        public static string StructSortKey(
            Element element, List<int> sortKeyIndices
        ) {
            string sortKey = "";
            if (sortKeyIndices == null) return sortKey;
            var container = (Container)element;
            sortKeyIndices.ForEach(index => {
                sortKey += container.elements[index].sortKey;
            });
            return sortKey;
        }
    }
}
