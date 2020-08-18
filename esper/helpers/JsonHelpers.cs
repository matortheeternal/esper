using esper.defs;
using esper.setup;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace esper.helpers {
    public class JsonHelpers {
        public static JObject ObjectAssign(
            JObject target, params JObject[] sources
        ) {
            foreach(JObject source in sources)
                target.Merge(source);
            return target;
        }

        public static List<T> List<T>(JObject src, string key) {
            if (!src.ContainsKey(key)) return null;
            return src.Value<JArray>(key).ToObject<List<T>>();
        }

        public static Dictionary<T1, T2> Dictionary<T1, T2>(JObject src, string key) {
            if (!src.ContainsKey(key)) return null;
            return src.Value<JObject>(key).ToObject<Dictionary<T1, T2>>();
        }

        public static ElementDef ElementDef(
            JObject src, string key, Def parent
        ) {
            ErrorHelpers.CheckDefProperty(src, key);
            var defSrc = src.Value<JObject>(key);
            return (ElementDef) parent.manager.BuildDef(defSrc);
        }

        public static ReadOnlyCollection<T> Defs<T>(
            DefinitionManager manager, JObject src, string key
        ) where T : Def {
            ErrorHelpers.CheckDefProperty(src, key);
            var sources = src.Value<JArray>(key);
            if (sources == null) throw new Exception("No def sources found.");
            return sources.Select(src => {
                return (T) manager.BuildDef((JObject)src);
            }).ToList().AsReadOnly();
        }

        public static Def Def(JObject src, string key, Def parent) {
            if (!src.ContainsKey(key)) return null;
            var defSrc = src.Value<JObject>(key);
            return parent.manager.BuildDef(defSrc);
        }

        public static FormatDef FormatDef(JObject src, Def parent) {
            if (!src.ContainsKey("format")) return null;
            var formatSrc = src.Value<JObject>("format");
            return (FormatDef)parent.manager.BuildDef(formatSrc);
        }

        public static Decider Decider(JObject src, Def parent) {
            ErrorHelpers.CheckDefProperty(src, "decider");
            return parent.manager.GetDecider(src.Value<string>("decider"));
        }
    }
}
