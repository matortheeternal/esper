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
    using ClassMap = Dictionary<string, Type>;
    using DeciderMap = Dictionary<string, Decider>;

    public class DefinitionManager {
        public Game game;
        public Session session;
        internal Def groupHeaderDef;
        private readonly JObject definitions;
        private readonly DefMap recordDefs = new DefMap();
        private readonly ClassMap defClasses = new ClassMap();
        private readonly DeciderMap deciders = new DeciderMap();
        private string defsFileName => game.abbreviation + ".json";
        public JArray groupOrder => definitions.Value<JArray>("groupOrder");

        public DefinitionManager(Game game, Session session) {
            this.game = game;
            this.session = session;
            definitions = IOHelpers.LoadResource(defsFileName);
            LoadClasses(game);
            if (!session.options.buildDefsOnDemand) BuildRecordDefs();
            var src = ResolveMetaDef("GroupRecordHeader");
            groupHeaderDef = BuildDef(src, null);
        }

        private void ForEachRecordDef(Action<string, JToken> action) {
            var defs = definitions.Value<JObject>("defs");
            foreach (var (key, def) in defs) {
                if (def.Value<string>("type") != "record") continue;
                action(key, def);
            }
        }

        private void BuildRecordDefs() {
            ForEachRecordDef((key, def) => {
                recordDefs[key] = BuildDef((JObject)def, null);
            });
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

        private void LoadDefClass(Type type) {
            var info = type.GetField("defType");
            if (info == null) return;
            string key = (string)info.GetValue(null);
            defClasses[key] = type;
        }

        public bool IsTopGroup(string sig) {
            foreach (var entry in groupOrder)
                if (entry.Value<string>() == sig) return true;
            return false;
        }

        private void LoadDecider(Type type) {
            string key = type.Name;
            deciders[key] = (Decider) Activator.CreateInstance(type);
        }
        
        private void LoadClass(Type type) {
            if (typeof(Def).IsAssignableFrom(type)) {
                LoadDefClass(type);
            } else if (typeof(Decider).IsAssignableFrom(type)) {
                LoadDecider(type);
            }
        }

        private void LoadClasses(Game game) {
            var gameDefsNamespace = "esper.defs." + game.defsNamespace;
            var types = ReflectionHelpers.GetClasses((Type t) => {
                return t.Namespace == "esper.defs" ||
                    t.Namespace == gameDefsNamespace;
            });
            foreach (var type in types) LoadClass(type);
        }

        public JObject MergeDef(JObject src) {
            var target = ResolveDef(src.Value<string>("id"));
            var result = new JObject();
            result.Merge(target);
            result.Merge(src);
            return result;
        }

        public Def BuildDef(JObject src, Def parent) {
            if (src.ContainsKey("id")) src = MergeDef(src);
            var defType = src.Value<string>("type");
            if (!defClasses.ContainsKey(defType))
                throw new Exception($"Def type not implemented: {defType}");
            var args = new object[] { this, src, parent };
            return (Def) Activator.CreateInstance(defClasses[defType], args);
        }

        public ReadOnlyCollection<ElementDef> BuildDefs(
            JArray sources, Def parent
        ) {
            if (sources == null) throw new Exception("No def sources found.");
            int sortOrder = 0;
            return sources.Select(src => {
                var def = (ElementDef) BuildDef((JObject)src, parent);
                def.sortOrder = sortOrder++;
                return def;
            }).ToList().AsReadOnly();
        }

        private void ApplyFlagsFormat(JObject headerDef, JObject src) {
            if (!src.ContainsKey("flags")) return;
            headerDef["elements"][2]["format"] = src["flags"];
        }

        public StructDef BuildMainRecordHeaderDef(JObject src, ElementDef recordDef) {
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

        public Decider GetDecider(string key) {
            if (!deciders.ContainsKey(key))
                throw new Exception("Unknown decider: " + key);
            return deciders[key];
        }
    }
}
