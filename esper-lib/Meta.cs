using esper.setup;
using Microsoft.JavaScript.NodeApi;

namespace esper_lib {
    [JSExport]
    public static class Meta {
        internal static Session session;
        internal static LoaderState loaderState = LoaderState.Inactive;
        internal static List<object?> store = [];
        internal static uint nextId = 0;

        // internal api
        private static void GetNextId() {
            var count = store.Count;
            nextId++;
            while (nextId < count) {
                if (store[(int)nextId] == null) return;
                nextId++;
            }
            nextId = 0;
        }

        internal static uint Store(object obj) {
            if (nextId > 0) {
                store[(int) nextId] = obj;
                var result = nextId;
                GetNextId();
                return result;
            } else {
                store.Add(obj);
                return (uint) (store.Count - 1);
            }
        }

        internal static object? Resolve(uint id, bool resolveRoot = false) {
            if (id == 0) {
                if (resolveRoot) return session.root;
                throw new Exception("Error: Cannot resolve NULL reference.");
            }
            return store[(int)id];
        }

        internal static uint StoreIfAssigned(object obj) {
            if (obj == null) throw new Exception("Not assigned");
            return Store(obj);
        }

        // public api
        [JSExport]
        public static void EndSession() {
            // TODO
        }

        [JSExport]
        public static void Release(uint id) {
            try {
                if (id == 0 || id >= store.Count || store[(int) id] == null) 
                    return;
                store[(int)id] = null;
            } catch (Exception e) {
                throw new JSException(e.Message);
            }
        }
    }
}
