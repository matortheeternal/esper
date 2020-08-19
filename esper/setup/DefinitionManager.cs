using esper.defs;
using esper.helpers;
using esper.data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace esper.setup {
    using DefMap = Dictionary<string, Def>;
    using ClassMap = Dictionary<string, Type>;
    using DeciderMap = Dictionary<string, Decider>;

    public class DefinitionManager {
        public Game game;
        public Session session;
        internal Def groupHeaderDef;
        internal List<string> groupOrder;
        private JObject definitions;

        private readonly DefMap defMap = new DefMap();
        private readonly ClassMap defClasses = new ClassMap();
        private readonly DeciderMap deciders = new DeciderMap();
        private string defsFileName => game.abbreviation + ".json";
        public CTDAFunctions ctdaFunctions => (CTDAFunctions) defMap["CTDAFunctions"];

        public DefinitionManager(Game game, Session session) {
            this.game = game;
            this.session = session;
            LoadDefinitions();
            LoadClasses();
            BuildDefs();
            groupOrder = JsonHelpers.List<string>(definitions, "groupOrder");
        }

        private void LoadDefinitions() {
            session.logger.Info($"Loading definitions from {defsFileName}");
            definitions = IOHelpers.LoadResource(defsFileName);
        }

        private void BuildDefs() {
            if (session.options.buildDefsOnDemand) return;
            var defs = definitions.Value<JObject>("defs");
            foreach (var (key, src) in defs)
                defMap[key] = BuildDef((JObject)src);
            var groupDefSrc = ResolveMetaDef("GroupRecordHeader");
            groupHeaderDef = BuildDef(groupDefSrc);
            definitions.Remove("defs");
            GC.Collect();
        }

        public void BuildRecordDef(string key) {
            var defs = definitions.Value<JObject>("defs");
            if (!defs.ContainsKey(key))
                throw new Exception("Def " + key + " not found.");
            var src = defs[key];
            if (src.Value<string>("type") != "record") 
                throw new Exception("Target def is not a record def.");
            defMap[key] = BuildDef((JObject)src);
        }

        private void LoadDefClass(Type type) {
            var info = type.GetField("defType");
            if (info == null) return;
            string key = (string)info.GetValue(null);
            defClasses[key] = type;
        }

        public bool IsTopGroup(string sig) {
            return groupOrder.Contains(sig);
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

        private void LoadClasses() {
            var gameDefsNamespace = "esper.defs." + game.defsNamespace;
            var types = ReflectionHelpers.GetClasses((Type t) => {
                return t.Namespace == "esper.defs" ||
                    t.Namespace == gameDefsNamespace;
            });
            foreach (var type in types) LoadClass(type);
        }

        public JObject MergeDef(JObject target, JObject src) {
            var result = new JObject();
            result.Merge(target);
            result.Merge(src);
            return result;
        }

        public Def BuildBaseDef(JObject src) {
            var defType = src.Value<string>("type");
            if (!defClasses.ContainsKey(defType))
                throw new Exception($"Def type not implemented: {defType}");
            var args = new object[] { this, src };
            var def = (Def)Activator.CreateInstance(defClasses[defType], args);
            return def;
        }

        public Def BuildDef(JObject src) {
            if (!src.ContainsKey("id")) 
                return BuildBaseDef(src);
            var id = src.Value<string>("id");
            var target = ResolveDefSource(id);
            if (src.Properties().Count() == 1) {
                if (!defMap.ContainsKey(id))
                    defMap[id] = BuildDef(target);
                return defMap[id];
            }
            src = MergeDef(target, src);
            return BuildBaseDef(src);
        }

        private void ApplyFlagsFormat(JObject headerDef, JObject src) {
            if (!src.ContainsKey("flags")) return;
            headerDef["elements"][2]["format"] = src["flags"];
        }

        public StructDef BuildMainRecordHeaderDef(JObject src, ElementDef recordDef) {
            JObject def = (JObject) ResolveMetaDef("MainRecordHeader").DeepClone();
            ApplyFlagsFormat(def, src);
            return (StructDef) BuildDef(def);
        }

        public JObject ResolveDefSource(string key) {
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
            if (session.options.buildDefsOnDemand && !defMap.ContainsKey(sig))
                BuildRecordDef(sig);
            return defMap[sig];
        }

        public Decider GetDecider(string key) {
            if (!deciders.ContainsKey(key))
                throw new Exception("Unknown decider: " + key);
            return deciders[key];
        }
    }
}
