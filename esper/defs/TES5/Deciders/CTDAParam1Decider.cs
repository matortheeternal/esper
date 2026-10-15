using esper.elements;
using esper.resolution;

namespace esper.defs.TES5 {
    public class CTDAParam1Decider : Decider {
        protected CTDAFunction GetCTDAFunction(Container container) {
            var function = container?.GetElement("Function") as ValueElement;
            if (function == null) return null;
            UInt16 index = Convert.ToUInt16(function.data?.ToInt64() ?? 0);
            var ctdaFunctions = container.manager.ctdaFunctions;
            return ctdaFunctions.FunctionByIndex(index);
        }

        internal virtual CTDAFunctionParamType? GetParamType(
            CTDAFunction ctdaFunction
        ) {
            return ctdaFunction.paramType1;
        }

        internal virtual int GetUnionIndex(CTDAFunctionParamType paramType) {
            return (int)paramType + 1;
        }

        protected long GetParamFlags(Container container) {
            var type = container?.GetElement("Type") as ValueElement;
            return type == null ? 0 : type.data?.ToInt64() ?? 0;
        }

        protected virtual CTDAFunctionParamType? ResolveParamType(
            Container container, CTDAFunction ctdaFunction
        ) {
            long paramFlag = GetParamFlags(container);
            var paramType = GetParamType(ctdaFunction);
            switch (paramType) {
                case CTDAFunctionParamType.ptObjectReference:
                case CTDAFunctionParamType.ptActor:
                case CTDAFunctionParamType.ptPackage:
                    if ((paramFlag & 0x2) > 0) {
                        return CTDAFunctionParamType.ptAlias;
                    } else if ((paramFlag & 0x8) > 0) {
                        return CTDAFunctionParamType.ptPackdata;
                    }
                    break;
            }
            return paramType;
        }

        public override int Decide(Container container) {
            if (container == null) return 0;
            var ctdaFunction = GetCTDAFunction(container);
            if (ctdaFunction == null) return 0;
            var paramType = ResolveParamType(container, ctdaFunction);
            if (paramType == null) return 0;
            return GetUnionIndex((CTDAFunctionParamType)paramType);
        }
    }
}
