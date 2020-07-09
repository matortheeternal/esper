using esper.helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace esper.setup {
    public class DefinitionManager {
        private JObject definitions;
        private Dictionary<string, Def> recordDefs;
        private Dictionary<string, Type> defClasses;

        public DefinitionManager(Game game) {
            ReadDefs(game);
            InitDefClasses(game);
            BuildRecordDefs();
        }

        private void BuildRecordDefs() {
            recordDefs = new Dictionary<string, Def>();
            var defs = definitions.Value<JObject>("defs");
            foreach (var (key, def) in defs) {
                if (def.Value<string>("type") != "record") continue;
                recordDefs[key] = BuildDef((JObject)def);
            }
        }

        private void InitDefClasses(Game game) {
            var gameDefsNamespace = "esper.defs" + game.abbreviation;
            ReflectionHelpers.BuildClassDictionary(defClasses, "defType", (Type t) => {
                return t.Namespace == "esper.defs" ||
                    t.Namespace == gameDefsNamespace;
            });
        }

        private void ReadDefs(Game game) {
            var filename = game.abbreviation + ".json";
            definitions = IOHelpers.LoadResource(filename);
        }

        public Def BuildDef(JObject src) {
            var defClass = defClasses[src.Value<string>("type")];
            return (Def) Activator.CreateInstance(defClass);
        }

        private void ApplyFlagsFormat(JObject headerDef, JObject src) {
            if (!src.ContainsKey("flags")) return;
            headerDef["elements"][2]["format"] = src["flags"];
        }

        public Def BuildMainRecordHeaderDef(JObject src) {
            JObject def = (JObject) ResolveMetaDef("MainRecordHeader").DeepClone();
            ApplyFlagsFormat(def, src);
            return BuildDef(def);
        }

        public JObject ResolveDef(string key) {
            var defs = definitions.Value<JObject>("defs");
            if (!defs.ContainsKey(key))
                throw new Exception("Unknown def: " + key);
            return defs.Value<JObject>(key);
        }

        public JObject ResolveMetaDef(string key) {
            var metaDefs = definitions.Value<JObject>("metaDefs");
            if (!metaDefs.ContainsKey(key))
                throw new Exception("Unknown meta def: " + key);
            return metaDefs.Value<JObject>(key);
        }
    }
}
