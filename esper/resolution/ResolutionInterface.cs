using esper.elements;
using esper.resolution.strategies;
using System;
using System.Collections.Generic;
using esper.helpers;

namespace esper.resolution {
    public interface IResolution {}

    public static class ResolutionExtensions {
        public static List<Type> strategies = new List<Type> {
            typeof(ResolveParent),
            typeof(ResolveReference),
            typeof(ResolveByIndex),
            typeof(ResolveBySignature),
            typeof(ResolveByName)
        };

        public static Element ResolveElement(this IResolution r, string pathPart) {
            var parameters = new object[] { r, pathPart };
            foreach (var strategy in strategies) {
                var matchMethod = strategy.GetMethod("Match");
                MatchData match = (MatchData)matchMethod.Invoke(null, parameters);
                if (match == null) continue;
                var resolveMethod = strategy.GetMethod("Resolve");
                return (Element)resolveMethod.Invoke(null, new object[] { match });
            }
            return null;
        }

        public static Element GetElement(this IResolution r, string path = "") {
            string pathPart;
            Element element = (Element) r;
            while (path.Length > 0) {
                if (element == null)
                    throw new Exception("Cannot resolve element from null.");
                StringHelpers.SplitPath(path, out pathPart, out path);
                element = element.ResolveElement(pathPart);
            }
            return element;
        }

        public static List<Element> GetElements(this IResolution r, string path = "") {
            Container container = (Container) r.GetElement(path);
            if (container == null) 
                throw new Exception("Element does not have child elements.");
            return container.elements;
        }

        public static string GetValue(this IResolution r, string path = "") {
            ValueElement valueElement = (ValueElement) r.GetElement(path);
            if (valueElement == null)
                throw new Exception("Element does not have a value.");
            return valueElement.value;
        }

        public static dynamic GetData(this IResolution r, string path = "") {
            ValueElement valueElement = (ValueElement) r.GetElement(path);
            if (valueElement == null)
                throw new Exception("Element does not have a value.");
            return valueElement.data;
        }
    }
}
