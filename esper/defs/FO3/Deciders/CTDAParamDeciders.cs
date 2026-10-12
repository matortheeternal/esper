using esper.elements;

namespace esper.defs.FO3 {
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
                CTDAFunctionParamType.ptQuestStage => 8,
                CTDAFunctionParamType.ptMiscStat => 9,
                CTDAFunctionParamType.ptAlignment => 10,
                CTDAFunctionParamType.ptEquipType => 11,
                CTDAFunctionParamType.ptFormType => 12,
                CTDAFunctionParamType.ptCriticalStage => 13,
                CTDAFunctionParamType.ptObjectReference => 14,
                CTDAFunctionParamType.ptInventoryObject => 15,
                CTDAFunctionParamType.ptActor => 16,
                CTDAFunctionParamType.ptVoiceType => 17,
                CTDAFunctionParamType.ptIdleForm => 18,
                CTDAFunctionParamType.ptFormList => 19,
                CTDAFunctionParamType.ptQuest => 20,
                CTDAFunctionParamType.ptFaction => 21,
                CTDAFunctionParamType.ptCell => 24,
                CTDAFunctionParamType.ptClass => 25,
                CTDAFunctionParamType.ptRace => 26,
                CTDAFunctionParamType.ptActorBase => 27,
                CTDAFunctionParamType.ptGlobal => 28,
                CTDAFunctionParamType.ptWeather => 29,
                CTDAFunctionParamType.ptPackage => 30,
                CTDAFunctionParamType.ptEncounterZone => 31,
                CTDAFunctionParamType.ptPerk => 32,
                CTDAFunctionParamType.ptOwner => 33,
                CTDAFunctionParamType.ptFurniture => 34,
                CTDAFunctionParamType.ptMagicItem => 35,
                CTDAFunctionParamType.ptMagicEffect => 36,
                CTDAFunctionParamType.ptWorldspace => 37,
                CTDAFunctionParamType.ptVATSValueFunction => 38,
                CTDAFunctionParamType.ptVATSValueParam => 39,
                CTDAFunctionParamType.ptReferencableObject => 44,
                CTDAFunctionParamType.ptRegion => 47,
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
