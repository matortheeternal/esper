using Newtonsoft.Json.Linq;

namespace esper.helpers {
    public static class ErrorHelpers {
        public static void CheckDefProperty(JObject src, string property) {
            if (src.ContainsKey(property)) return;
            throw new System.Exception($"Expected def property {property}");
        }
    }
}
