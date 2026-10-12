using esper.elements;

namespace esper.defs.TES4 {
    public class CTDAParam1Decider : TES5.CTDAParam1Decider {
        protected override CTDAFunctionParamType? ResolveParamType(
            Container container, CTDAFunction ctdaFunction
        ) {
            return GetParamType(ctdaFunction);
        }

        internal override int GetUnionIndex(CTDAFunctionParamType paramType) {
            return paramType switch {
                CTDAFunctionParamType.ptNone => 1,
                CTDAFunctionParamType.ptInteger => 2,
                CTDAFunctionParamType.ptVariableName => 3,
                CTDAFunctionParamType.ptSex => 4,
                CTDAFunctionParamType.ptActorValue => 5,
                CTDAFunctionParamType.ptCrimeType => 6,
                CTDAFunctionParamType.ptAxis => 7,
                CTDAFunctionParamType.ptFormType => 8,
                CTDAFunctionParamType.ptQuestStage => 9,
                CTDAFunctionParamType.ptObjectReference => 10,
                CTDAFunctionParamType.ptInventoryObject => 11,
                CTDAFunctionParamType.ptActor => 12,
                CTDAFunctionParamType.ptQuest => 13,
                CTDAFunctionParamType.ptFaction => 14,
                CTDAFunctionParamType.ptCell => 15,
                CTDAFunctionParamType.ptClass => 16,
                CTDAFunctionParamType.ptRace => 17,
                CTDAFunctionParamType.ptActorBase => 18,
                CTDAFunctionParamType.ptGlobal => 19,
                CTDAFunctionParamType.ptWeather => 20,
                CTDAFunctionParamType.ptPackage => 21,
                CTDAFunctionParamType.ptOwner => 22,
                CTDAFunctionParamType.ptFurniture => 24,
                CTDAFunctionParamType.ptMagicItem => 25,
                CTDAFunctionParamType.ptMagicEffect => 26,
                CTDAFunctionParamType.ptWorldspace => 27,
                CTDAFunctionParamType.ptReferencableObject => 28,
                _ => 0
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
