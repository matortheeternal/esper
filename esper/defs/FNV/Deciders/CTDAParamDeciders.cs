using esper.elements;

namespace esper.defs.FNV {
    public class CTDAParam1Decider : FO3.CTDAParam1Decider {
        internal override int GetUnionIndex(CTDAFunctionParamType paramType) {
            return paramType switch {
                CTDAFunctionParamType.ptQuestObjective => 45,
                CTDAFunctionParamType.ptReputation => 46,
                CTDAFunctionParamType.ptChallenge => 48,
                CTDAFunctionParamType.ptCasino => 49,
                CTDAFunctionParamType.ptAnyForm => 50,
                _ => base.GetUnionIndex(paramType),
            };
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
