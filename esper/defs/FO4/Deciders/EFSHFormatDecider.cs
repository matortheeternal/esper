using esper.elements;
using esper.resolution;

namespace esper.defs.FO4 {
    // Port of xEdit wbEFSHFormatDecider: EFSH records with form version
    // below 102 use the old DNAM format.
    public class EFSHFormatDecider : Decider {
        public override int Decide(Container container) {
            if (!(container is MainRecord)) return 0;
            int? formVer = (int?)container.GetData<int?>(
                @"Record Header\Form Version");
            if (formVer == null) return 0;
            return formVer < 102 ? 1 : 0;
        }
    }
}
