using esper.setup;

namespace esper_lib {
    public class Meta {
        internal static Session session;
        internal static LoaderState loaderState = LoaderState.Inactive;
        internal static List<object> store = new List<object>();
        internal static uint nextId = 0;

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

        internal static object Resolve(uint id, bool resolveRoot = false) {
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
    }
}
