namespace esper.defs.TES5 {
    public class CTDAParam2Decider : CTDAParam1Decider {
        internal override CTDAFunctionParamType? GetParamType(
            CTDAFunction ctdaFunction
        ) {
            return ctdaFunction.paramType2;
        }
    }
}
