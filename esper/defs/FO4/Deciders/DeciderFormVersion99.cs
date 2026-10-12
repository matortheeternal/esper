using esper.elements;
using esper.resolution;

namespace esper.defs.FO4 {
    // Port of xEdit wbFormVerDecider(99): records with form version 99+ use
    // the new format. Defaults to 1 when the header is unavailable.
    public class DeciderFormVersion99 : Decider {
        public override int Decide(Container container) {
            int? formVer = (int?)container?.record?.GetData<int?>(
                @"Record Header\Form Version");
            if (formVer == null) return 1;
            return formVer >= 99 ? 1 : 0;
        }
    }
}
