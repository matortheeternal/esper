using esper.elements;
using esper.resolution.strategies;
using System;
using System.Collections.Generic;

namespace esper.resolution {
    public class ResolutionInterface {
        public static List<Type> strategies = new List<Type> {
            typeof(ResolveParent),
            typeof(ResolveReference),
            typeof(ResolveByIndex),
            typeof(ResolveBySignature),
            typeof(ResolveByName)
        };

        private void SplitPath(string path, out string pathPart, out string remainingPath) {
            int separatorIndex = path.IndexOf(@"\");
            if (separatorIndex == -1) {
                pathPart = path;
                remainingPath = "";
                return;
            }
            pathPart = path.Substring(0, separatorIndex);
            remainingPath = path.Substring(separatorIndex + 1, path.Length);
        }

        public Element ResolveElement(string pathPart) {
            var parameters = new object[] { this, pathPart };
            foreach (var strategy in strategies) {
                var matchMethod = strategy.GetMethod("Match");
                MatchData match = (MatchData)matchMethod.Invoke(null, parameters);
                if (match == null) continue;
                var resolveMethod = strategy.GetMethod("Resolve");
                return (Element)resolveMethod.Invoke(null, new object[] { match });
            }
            return null;
        }

        public Element GetElement(string path) {
            string pathPart;
            Element element = (Element) this;
            while (path.Length > 0) {
                if (element == null)
                    throw new Exception("Cannot resolve element from null.");
                SplitPath(path, out pathPart, out path);
                element = element.ResolveElement(pathPart);
            }
            return element;
        }

        public List<Element> GetElements(string path) {
            Container container = (Container) GetElement(path);
            if (container == null) 
                throw new Exception("Element does not have child elements.");
            return container.elements;
        }

        public string GetValue(string path) {
            ValueElement valueElement = (ValueElement)GetElement(path);
            if (valueElement == null)
                throw new Exception("Element does not have a value.");
            return valueElement.value;
        }
    }
}
