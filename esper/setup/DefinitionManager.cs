using esper.defs;
using esper.helpers;
using esper.parsing;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace esper.setup {
    using DefMap = Dictionary<string, Def>;
    using DefClassMap = Dictionary<string, Type>;

    public class DefinitionManager {
        public Game game;
        public Session session;
        private readonly JObject definitions;
        private readonly DefMap recordDefs = new DefMap();
        private readonly DefClassMap defClasses = new DefClassMap();
        private string defsFileName { get => game.abbreviation + ".json"; }

        public DefinitionManager(Game game, Session session) {
            this.game = game;
            this.session = session;
            definitions = IOHelpers.LoadResource(defsFileName);
            InitDefClasses(game);
            if (session.options.buildDefsOnDemand) return;
            BuildRecordDefs();
        }

        private void BuildRecordDefs() {
            var defs = definitions.Value<JObject>("defs");
            foreach (var (key, def) in defs) {
                if (def.Value<string>("type") != "record") continue;
                recordDefs[key] = BuildDef((JObject)def, null);
            }
        }

        public void BuildRecordDef(string key) {
            var defs = definitions.Value<JObject>("defs");
            if (!defs.ContainsKey(key))
                throw new Exception("Def " + key + " not found.");
            var def = defs[key];
            if (def.Value<string>("type") != "record") 
                throw new Exception("Target def is not a record def.");
            recordDefs[key] = BuildDef((JObject)def, null);
        }

        private void InitDefClasses(Game game) {
            var gameDefsNamespace = "esper.defs." + game.abbreviation;
            ReflectionHelpers.BuildClassDictionary(defClasses, "defType", (Type t) => {
                return t.Namespace == "esper.defs" ||
                    t.Namespace == gameDefsNamespace;
            });
        }

        public Def BuildDef(JObject src, Def parent) {
            var defClass = defClasses[src.Value<string>("type")];
            var args = new object[] { this, src, parent };
            return (Def) Activator.CreateInstance(defClass, args);
        }

        public ReadOnlyCollection<Def> BuildDefs(JArray sources, Def parent) {
            if (sources == null) throw new Exception("No def sources found.");
            return sources.Select(
                src => BuildDef((JObject)src, parent)
            ).ToList().AsReadOnly();
        }

        private void ApplyFlagsFormat(JObject headerDef, JObject src) {
            if (!src.ContainsKey("flags")) return;
            headerDef["elements"][2]["format"] = src["flags"];
        }

        public StructDef BuildMainRecordHeaderDef(JObject src, Def recordDef) {
            JObject def = (JObject) ResolveMetaDef("MainRecordHeader").DeepClone();
            ApplyFlagsFormat(def, src);
            return (StructDef) BuildDef(def, recordDef);
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
            var sig = signature.ToString();
            if (session.options.buildDefsOnDemand && !recordDefs.ContainsKey(sig))
                BuildRecordDef(sig);
            return recordDefs[sig];
        }
    }
}
