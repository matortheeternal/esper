using esper.elements;

namespace esper.defs.FNV {
    public class CTDAParam1Decider : FO3.CTDAParam1Decider {
        internal override int GetUnionIndex(CTDAFunctionParamType paramType) {
            if (paramType == CTDAFunctionParamType.ptQuestObjective)
                return 45;
            return base.GetUnionIndex(paramType);
        }
    }

    public class CTDAParam2Decider : CTDAParam1Decider {
        internal override CTDAFunctionParamType? GetParamType(
            CTDAFunction ctdaFunction
        ) {
            return ctdaFunction.paramType2;
        }
    }
}
