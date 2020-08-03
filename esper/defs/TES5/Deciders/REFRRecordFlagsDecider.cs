using esper.elements;
using esper.resolution;

namespace esper.defs.TES5 {
    public class REFRRecordFlagsDecider : Decider {
        public override int Decide(Container container) {
            var baseRec = (MainRecord) container?.GetElement(@"..\@NAME");
            return baseRec?.signature switch {
                "ACTI" => 1, "STAT" => 1, "TREE" => 1,
                "CONT" => 2,
                "DOOR" => 3,
                "LIGH" => 4,
                "MSTT" => 5,
                "ADDN" => 6,
                "SCRL" => 7, "AMMO" => 7, "ARMO" => 7, "BOOK" => 7,
                "INGR" => 7, "KEYM" => 7, "MISC" => 7, "SLGM" => 7,
                "WEAP" => 7, "ALCH" => 7,
                _ => 0
            };
        }
    }
}
