using esper.defs;
using esper.helpers;
using esper.parsing;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace esper.setup {
    public class DefinitionManager {
        public Game game;
        public Session session;
        private JObject definitions;
        private Dictionary<string, Def> recordDefs;
        private Dictionary<string, Type> defClasses;

        public DefinitionManager(Game game, Session session) {
            this.game = game;
            this.session = session;
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

        public Def BuildDef(JObject src, Def parent = null) {
            var defClass = defClasses[src.Value<string>("type")];
            var args = new object[] { this, src, parent };
            return (Def) Activator.CreateInstance(defClass, args);
        }

        public List<Def> BuildDefs(JArray sources, Def parent) {
            if (sources == null) throw new Exception("No def sources found.");
            return sources.Select(src => BuildDef((JObject)src, parent)).ToList();
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

        public Def GetRecordDef(Signature signature) {
            // TODO: lookup by signature maybe?
            return recordDefs[signature.ToString()];
        }
    }
}
