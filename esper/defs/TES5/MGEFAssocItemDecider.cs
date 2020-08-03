using esper.elements;
using esper.resolution;

namespace esper.defs.TES5 {
    public class MGEFAssocItemDecider : Decider {
        public override int Decide(Container container) {
            var e = container.GetElement("Archtype");
            if (e == null) return 0;
            return e.GetData() switch {
                12 => 1, // Light
                17 => 2, // Bound Item
                18 => 3, // Summon Creature
                25 => 4, // Guide
                34 => 8, // Peak Mod
                35 => 5, // Cloak
                36 => 6, // Werewolf
                39 => 7, // Enhance Weapon
                40 => 4, // Spawn Hazard
                46 => 6, // Vampire Lord
                _ => 0
            };
        }
    }
}
