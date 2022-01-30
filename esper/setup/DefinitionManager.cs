using esper.data;
using esper.defs;
using esper.helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace esper.setup {
    using DefMap = Dictionary<string, Def>;
    using ClassMap = Dictionary<string, Type>;
    using DeciderMap = Dictionary<string, Decider>;
    using RecordDefMap = Dictionary<int, ElementDef>;

    public class DefinitionManager {
        public Game game;
        public Session session;
        internal GroupManager groupManager;
        private JObject definitions;
        private readonly DefMap defMap = new DefMap();
        private readonly RecordDefMap recordDefMap = new RecordDefMap();
        private readonly ClassMap defClasses = new ClassMap();
        internal StructDef recordHeaderDef;
        internal StructDef groupHeaderDef;
        private readonly DeciderMap deciders = new DeciderMap();

        public CTDAFunctions ctdaFunctions => (CTDAFunctions) defMap["CTDAFunctions"];

        public DefinitionManager(Game game, Session session) {
            this.game = game;
            this.session = session;
            groupManager = new GroupManager(this);
            LoadDefinitions();
            LoadClasses();
            BuildDefs();
        }

        private void LoadDefinitions() {
            var defsFileName = game.abbreviation + ".json";
            session.logger.Info($"Loading definitions from {defsFileName}");
            definitions = IOHelpers.LoadDefinitions(defsFileName);
        }

        private void BuildDefs() {
            var defs = definitions.Value<JObject>("defs");
            recordHeaderDef = (StructDef)BuildDef((JObject) defs["MainRecordHeader"]);
            groupHeaderDef = (StructDef)BuildDef((JObject) defs["GroupRecordHeader"]);
            foreach (var (key, src) in defs) {
                if (src.Value<string>("type") == "record") {
                    var sig = Signature.FromString(key);
                    recordDefMap[sig.v] = (ElementDef)BuildDef((JObject)src);
                } else {
                    defMap[key] = BuildDef((JObject)src);
                }
            }
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
            var sig = Signature.FromString(key);
            recordDefMap[sig.v] = (ElementDef) BuildDef((JObject)src);
        }

        private void LoadDefClass(Type type) {
            var info = type.GetField("defId");
            if (info == null) return;
            string key = (string)info.GetValue(null);
            defClasses[key] = type;
        }

        private void LoadDecider(Type type) {
            string key = type.Name;
            deciders[key] = (Decider) Activator.CreateInstance(type);
        }
        
        private void LoadClass(Type type) {
            if (typeof(GroupDef).IsAssignableFrom(type)) {
                groupManager.LoadGroupDefClass(type);
            } else if (typeof(Def).IsAssignableFrom(type)) {
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
            var defId = src.Value<string>("type");
            if (defId == "record" && !src.ContainsKey("signature")) return null;
            if (defId == "group") return groupManager.BuildGroupDef(src);
            if (!defClasses.ContainsKey(defId))
                throw new Exception($"Def type not implemented: {defId}");
            var args = new object[] { this, src };
            var def = (Def)Activator.CreateInstance(defClasses[defId], args);
            return def;
        }

        public Def BuildDef(JObject src) {
            if (!src.ContainsKey("id")) 
                return BuildBaseDef(src);
            var id = src.Value<string>("id");
            var target = ResolveDefSource(id);
            if (src.Properties().Count() == 1) 
                return ResolveDef(id);
            src = MergeDef(target, src);
            return BuildBaseDef(src);
        }

        public JObject ResolveDefSource(string key) {
            var defs = definitions.Value<JObject>("defs");
            if (!defs.ContainsKey(key))
                throw new Exception("Unknown def: " + key);
            return defs.Value<JObject>(key);
        }

        internal Def ResolveDef(string id) {
            if (!defMap.ContainsKey(id))
                defMap[id] = BuildDef(ResolveDefSource(id));
            return defMap[id];
        }

        public ElementDef GetRecordDef(Signature sig) {
            return recordDefMap[sig.v];
        }

        public Decider GetDecider(string key) {
            if (!deciders.ContainsKey(key))
                throw new Exception("Unknown decider: " + key);
            return deciders[key];
        }
    }
}
