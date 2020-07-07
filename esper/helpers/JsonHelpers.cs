using Newtonsoft.Json.Linq;

namespace esper {
    public class JsonHelpers {
        public static JObject ObjectAssign(JObject target, params JObject[] sources) {
            foreach(JObject source in sources)
                target.Merge(source);
            return target;
        }
    }
}
