using esper.elements;
using esper.resolution;

namespace esper.defs.FO4 {
    // Port of xEdit wbSceneActionSoundDecider: scene actions whose type is
    // not 4 (Play Sound) hold an end-scene greeting instead of a sound.
    public class SceneActionSoundDecider : Decider {
        public override int Decide(Container container) {
            int? type = (int?)container?.GetData<int?>("ANAM");
            if (type == null) return 0;
            return type != 4 ? 1 : 0;
        }
    }
}
