using esper.setup;

namespace esper.defs.FO4.Counters {
    public class OffsetDataColsCounter : TES5.OffsetDataColsCounter {
        public OffsetDataColsCounter(
            DefinitionManager manager, JObject src
        ) : base(manager, src) { }
    }

    public class ScriptFragmentsInfoCounter : TES5.ScriptFragmentsInfoCounter {
        public ScriptFragmentsInfoCounter(
            DefinitionManager manager, JObject src
        ) : base(manager, src) { }
    }

    public class ScriptFragmentsPackCounter : TES5.ScriptFragmentsPackCounter {
        public ScriptFragmentsPackCounter(
            DefinitionManager manager, JObject src
        ) : base(manager, src) { }
    }

    public class ScriptFragmentsQuestCounter : TES5.ScriptFragmentsQuestCounter {
        public ScriptFragmentsQuestCounter(
            DefinitionManager manager, JObject src
        ) : base(manager, src) { }
    }

    public class ScriptFragmentsSceneCounter : TES5.ScriptFragmentsSceneCounter {
        public ScriptFragmentsSceneCounter(
            DefinitionManager manager, JObject src
        ) : base(manager, src) { }
    }
}
