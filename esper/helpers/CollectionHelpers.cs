using System;
using System.Collections.Generic;

namespace esper.helpers {
    public static class CollectionHelpers {
        public static T1 BinarySearch<T1>(
            List<T1> list, Func<T1, int> compare, bool returnLast = false
        ) {
            int low = 0;
            int high = list.Count;
            int test = high / 2;
            while (high - low > 0) {
                T1 entry = list[test];
                int comparison = compare(entry);
                if (comparison < 0) {
                    high = test;
                    test = low + (high - low) / 2;
                } else if (comparison > 0) {
                    low = test;
                    test = low + (high - low) / 2;
                } else {
                    return entry;
                }
            }
            return returnLast ? list[test] : default;
        }
    }
}
