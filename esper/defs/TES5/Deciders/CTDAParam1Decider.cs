using esper.elements;
using esper.resolution;
using System;

namespace esper.defs.TES5 {
    public class CTDAParam1Decider : Decider {
        protected CTDAFunction GetCTDAFunction(Container container) {
            UInt16 index = container.GetData("Function");
            var ctdaFunctions = container.manager.ctdaFunctions;
            return ctdaFunctions.FunctionByIndex(index);
        }

        internal virtual CTDAFunctionParamType? GetParamType(
            CTDAFunction ctdaFunction
        ) {
            return ctdaFunction.paramType1;
        }

        protected CTDAFunctionParamType? ResolveParamType(
            Container container, CTDAFunction ctdaFunction
        ) {
            long paramFlag = container.GetData("Type");
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
            return (int)paramType + 1;
        }
    }
}
