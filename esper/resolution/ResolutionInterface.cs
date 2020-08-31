using esper.elements;
using esper.resolution.strategies;
using System;
using System.Collections.Generic;
using esper.helpers;
using esper.defs;
using System.Collections.ObjectModel;

namespace esper.resolution {
    using ResolutionStrategies = List<ResolutionStrategy>;

    public interface IResolution {
        public Container container { get; }
    }

    public static class ResolutionExtensions {
        public static ResolutionStrategies strategies = new ResolutionStrategies {
            new ResolveContainer(),
            new ResolveParent(),
            new ResolveReference(),
            new ResolveByIndex(),
            new ResolveBySignature(),
            new ResolveByName()
        };

        public static Element ResolveElement(this IResolution r, string pathPart) {
            foreach (var strategy in strategies) {
                MatchData match = strategy.Match((Element) r, pathPart);
                if (match == null) continue;
                return strategy.Resolve(match);
            }
            return null;
        }

        public static Element CreateElement(this IResolution r, string pathPart) {
            foreach (var strategy in strategies) {
                MatchData match = strategy.Match((Element)r, pathPart);
                if (match == null) continue;
                return strategy.Resolve(match) ?? strategy.Create(match);
            }
            return null;
        }

        public static Element GetElement(this IResolution r, string path = "") {
            Element element = (Element)r;
            while (path.Length > 0) {
                if (element == null) return null;
                StringHelpers.SplitPath(path, out string pathPart, out path);
                element = element.ResolveElement(pathPart);
            }
            return element;
        }

        public static Element GetElementEx(this IResolution r, string path = "") {
            Element element = (Element)r;
            if (element == null)
                throw new Exception("Cannot resolve element from null.");
            while (path.Length > 0) {
                StringHelpers.SplitPath(path, out string pathPart, out path);
                element = element.ResolveElement(pathPart);
                if (element == null)
                    throw new Exception($"Failed to resolve element {pathPart}");
            }
            return element;
        }

        public static ReadOnlyCollection<Element> GetElements(this IResolution r, string path = "") {
            Container container = (Container)r.GetElement(path);
            return container?.elements;
        }

        public static ReadOnlyCollection<Element> GetElementsEx(this IResolution r, string path = "") {
            Container container = (Container) r.GetElementEx(path);
            if (container == null) 
                throw new Exception("Element does not have child elements.");
            return container.elements;
        }

        public static string GetValue(this IResolution r, string path = "") {
            ValueElement valueElement = (ValueElement) r.GetElement(path);
            return valueElement?.value;
        }

        public static string GetValueEx(this IResolution r, string path = "") {
            ValueElement valueElement = (ValueElement)r.GetElementEx(path);
            if (valueElement == null)
                throw new Exception("Element does not have a value.");
            return valueElement.value;
        }

        public static dynamic GetData(this IResolution r, string path = "") {
            ValueElement valueElement = (ValueElement) r.GetElement(path);
            return valueElement?.data;
        }

        public static string GetDataEx(this IResolution r, string path = "") {
            ValueElement valueElement = (ValueElement)r.GetElementEx(path);
            if (valueElement == null)
                throw new Exception("Element does not have a value.");
            return valueElement.data;
        }

        public static bool GetFlag(this IResolution r, string flagsPath, string flag) {
            ValueElement valueElement = (ValueElement)r.GetElement(flagsPath);
            FlagsDef flagsDef = valueElement?.flagsDef;
            if (flagsDef == null) return false;
            return flagsDef.FlagIsSet(valueElement.data, flag);
        }

        public static bool GetFlagEx(this IResolution r, string flagsPath, string flag) {
            ValueElement valueElement = (ValueElement)r.GetElementEx(flagsPath);
            if (valueElement == null)
                throw new Exception("Element does not have a value.");
            FlagsDef flagsDef = valueElement.flagsDef;
            if (flagsDef == null)
                throw new Exception("Element does not have flags.");
            return flagsDef.FlagIsSet(valueElement.data, flag);
        }

        public static Element GetParentElement(
            this IResolution r, Func<Element, bool> test
        ) {
            var parent = r.container;
            while (parent != null) {
                if (test(parent)) return parent;
                parent = parent.container;
            }
            return null;
        }

        public static void SetData(this IResolution r, string path, dynamic data) {
            ValueElement valueElement = (ValueElement)r.GetElement(path);
            if (valueElement == null) return;
            valueElement.data = data;
        }

        public static Element AddElement(this IResolution r, string path) {
            Element element = (Element)r;
            while (path.Length > 0) {
                if (element == null) return null;
                StringHelpers.SplitPath(path, out string pathPart, out path);
                element = element.CreateElement(pathPart);
            }
            return element;
        }
    }
}
