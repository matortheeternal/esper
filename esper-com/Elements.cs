using esper.elements;
using esper.resolution;
using System;

namespace esperlib {
    public class Elements {
        public static Element GetElementEx(uint id, string path) {
            Element e = (Element)Meta.Resolve(id, true);
            return e.GetElementEx(path);
        }

        public static Element GetElement(uint id, string path) {
            Element e = (Element) Meta.Resolve(id, true);
            return e.GetElement(path);
        }

        unsafe public bool HasElement(uint id, string path, bool* result) {
            try {
                var e = GetElement(id, path);
                *result = e != null;
                return true;
            } catch (Exception e) {
                Errors.HandleException(e);
            }
            return false;
        }

        unsafe public bool GetElement(uint id, string path, uint* result) {
            try {
                var e = GetElementEx(id, path);
                *result = Meta.Store(e);
                return true;
            } catch (Exception e) {
                Errors.HandleException(e);
            }
            return false;
        }

        unsafe public bool AddElement(uint id, string path, uint* result) {
            try {
                Element e = (Element)Meta.Resolve(id, true);
                var r = e.AddElement(path);
                *result = Meta.Store(r);
                return true;
            } catch (Exception e) {
                Errors.HandleException(e);
            }
            return false;
        }
    }
}
