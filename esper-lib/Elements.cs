using esper.elements;
using esper.resolution;
using Microsoft.JavaScript.NodeApi;

namespace esper_lib {
    [JSExport]
    public static class Elements {
        // internal private api
        internal static Element GetElementEx(uint id, string path) {
            Element e = (Element) Meta.Resolve(id, true);
            return e.GetElementEx(path);
        }

        internal static Element NativeGetElement(uint id, string path) {
            Element e = (Element) Meta.Resolve(id, true);
            return e.GetElement(path);
        }

        // public api
        [JSExport]
        public static bool HasElement(uint id, string path) {
            try {
                var e = NativeGetElement(id, path);
                return e != null;
            } catch (Exception e) {
                throw new JSException(e.Message);
            }
        }

        [JSExport]
        public static uint GetElement(uint id, string path) {
            try {
                var e = GetElementEx(id, path);
                return Meta.Store(e);
            } catch (Exception e) {
                throw new JSException(e.Message);
            }
        }

        [JSExport]
        public static uint AddElement(uint id, string path) {
            try {
                Element e = (Element)Meta.Resolve(id, true);
                var r = e.AddElement(path);
                if (r == null) 
                    throw new Exception("Failed to add element at path: " + path);
                return Meta.Store(r);
            } catch (Exception e) {
                throw new JSException(e.Message);
            }
        }

        [JSExport]
        public static uint AddElementValue(uint id, string path, string value) {
            try {
                Element e = (Element)Meta.Resolve(id, true);
                var r = e.AddElement(path);
                if (r == null)
                    throw new Exception("Failed to add element at path: " + path);
                ValueElement v = (ValueElement)r;
                v.value = value;
                return Meta.Store(r);
            } catch (Exception e) {
                throw new JSException(e.Message);
            }
        }

        [JSExport]
        public static void RemoveElement(uint id, string path) {
            try {
                var e = GetElementEx(id, path);
                e.Remove();
            } catch (Exception e) {
                throw new JSException(e.Message);
            }
        }

        [JSExport]
        public static void RemoveElementOrParent(uint id) {
            try {
                Element e = (Element)Meta.Resolve(id, true);
                while (!e.Remove()) {
                    if (e is MainRecord)
                        throw new Exception("Reached main record - could not remove");
                    e = e.container;
                }
            } catch (Exception e) {
                throw new JSException(e.Message);
            }
        }

        [JSExport]
        public static void SetElement(uint id, uint id2) {
            try {
                Element e = (Element)Meta.Resolve(id, false);
                Element e2 = (Element)Meta.Resolve(id2, false);
                //e.ReplaceWith(e2);
                // TODO: IMPLEMENT THIS
            } catch (Exception e) {
                throw new JSException(e.Message);
            }
        }

        [JSExport]
        public static uint[] GetElements(uint id, string path, bool sort, bool filter, bool sparse) {
            try {
                var c = GetElementEx(id, path);
                return c.GetElements().Select(e => Meta.Store(e)).ToArray();
            } catch (Exception e) {
                throw new JSException(e.Message);
            }
        }

        [JSExport]
        public static string[] GetDefNames(uint id) {
            try {
                Element e = (Element)Meta.Resolve(id, false);
                if (e.def.childDefs != null)
                    return e.def.childDefs.Select(d => d.name).ToArray();
                return [e.def.name];
            } catch (Exception e) {
                throw new JSException(e.Message);
            }
        }
    }
}
