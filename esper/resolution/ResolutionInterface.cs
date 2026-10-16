using esper.defs;
using esper.elements;
using esper.helpers;
using esper.resolution.strategies;
using System.Collections.ObjectModel;

namespace esper.resolution {
    using ResolutionStrategies = List<ResolutionStrategy>;

    [JSExport]
    public static class ResolutionExtensions {
        public static ResolutionStrategies strategies = new ResolutionStrategies {
            new CreateByDefault(),
            new ResolveContainer(),
            new ResolveParent(),
            new ResolveByFileFormId(),
            new ResolveReference(),
            new ResolveByIndex(),
            new ResolveFirst(),
            new ResolveBySignature(),
            new ResolveByName()
        };

        public static Element ResolveElement(this Element r, string pathPart) {
            foreach (var strategy in strategies) {
                if (!strategy.canResolve) continue;
                MatchData match = strategy.Match(r, pathPart);
                if (match == null) continue;
                return strategy.Resolve(match);
            }
            return null;
        }

        public static Element CreateElement(this Element r, string pathPart) {
            foreach (var strategy in strategies) {
                MatchData match = strategy.Match(r, pathPart);
                if (match == null) continue;
                if (!strategy.canResolve) return strategy.Create(match);
                return strategy.Resolve(match) ?? strategy.Create(match);
            }
            return null;
        }

        public static Element GetElement(this Element r, string path = "") {
            var element = r;
            while (path.Length > 0) {
                if (element == null) return null;
                StringHelpers.SplitPath(path, out string pathPart, out path);
                element = element.ResolveElement(pathPart);
            }
            return element;
        }

        public static Element GetElementEx(this Element r, string path = "") {
            var element = r;
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

        public static List<Element> GetElements(this Element r, string path = "") {
            Container container = (Container)r.GetElement(path);
            return container?.elements;
        }

        public static List<Element> GetElementsEx(this Element r, string path = "") {
            var container = r.GetElementEx(path) as Container;
            if (container == null)
                throw new Exception("Element does not have child elements.");
            return container.elements;
        }

        public static string GetValue(this Element r, string path = "") {
            var valueElement = r.GetElement(path) as ValueElement;
            return valueElement?.value;
        }

        public static string GetValueEx(this Element r, string path = "") {
            var valueElement = r.GetElementEx(path) as ValueElement;
            if (valueElement == null)
                throw new Exception("Element does not have a value.");
            return valueElement.value;
        }

        public static T GetData<T>(this Element r, string path = "") {
            ValueElement valueElement = (ValueElement)r.GetElement(path);
            var data = valueElement?.data;
            if (data == null) return (T)(object)null;
            return data.As<T>();
        }

        public static T GetDataEx<T>(this Element r, string path = "") {
            var valueElement = r.GetElementEx(path) as ValueElement;
            if (valueElement == null)
                throw new Exception("Element does not have a value.");
            var data = valueElement.data;
            if (data == null) return (T)(object)null;
            return data.As<T>();
        }

        public static bool GetFlag(this Element r, string flagsPath, string flag) {
            var valueElement = r.GetElement(flagsPath) as ValueElement;
            var flagsDef = valueElement?.flagsDef;
            if (flagsDef == null) return false;
            return flagsDef.FlagIsSet(valueElement.data, flag);
        }

        public static bool GetFlagEx(this Element r, string flagsPath, string flag) {
            var valueElement = r.GetElementEx(flagsPath) as ValueElement;
            if (valueElement == null)
                throw new Exception("Element does not have a value.");
            FlagsDef flagsDef = valueElement.flagsDef;
            if (flagsDef == null)
                throw new Exception("Element does not have flags.");
            return flagsDef.FlagIsSet(valueElement.data, flag);
        }

        public static void SetFlag(this Element r, string flagsPath, string flag, bool state) {
            var valueElement = r.GetElement(flagsPath) as ValueElement;
            var flagsDef = valueElement?.flagsDef;
            if (flagsDef == null) return;
            flagsDef.SetFlag(valueElement, flag, state);
        }

        public static void SetFlagEx(this Element r, string flagsPath, string flag, bool state) {
            var valueElement = r.GetElementEx(flagsPath) as ValueElement;
            if (valueElement == null)
                throw new Exception("Element does not have a value.");
            FlagsDef flagsDef = valueElement.flagsDef;
            if (flagsDef == null)
                throw new Exception("Element does not have flags.");
            flagsDef.SetFlag(valueElement, flag, state);
        }

        public static Element GetParentElement(
            this Element r, Func<Element, bool> test
        ) {
            var parent = r.container;
            while (parent != null) {
                if (test(parent)) return parent;
                parent = parent.container;
            }
            return null;
        }

        public static void SetData<T>(this Element r, string path, T data) {
            var valueElement = r.GetElement(path) as ValueElement;
            if (valueElement == null) return;
            valueElement.data = VariantData.From(data);
        }

        public static void SetValue(
            this Element r, string path, string value
        ) {
            if (r.GetElement(path) is ValueElement valueElement)
                valueElement.value = value;
        }

        public static Element AddElement(this Element r, string path) {
            Element element = r;
            while (path.Length > 0) {
                if (element == null) return null;
                StringHelpers.SplitPath(path, out string pathPart, out path);
                element = element.CreateElement(pathPart);
            }
            return element;
        }

        public static bool RemoveElement(this Element r, string path) {
            var element = r.GetElement(path);
            if (element == null) return false;
            return element.Remove();
        }
    }
}
