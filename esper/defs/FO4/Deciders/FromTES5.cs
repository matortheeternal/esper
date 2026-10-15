using esper.data;
using esper.elements;
using esper.resolution;

namespace esper.defs.FO4.Deciders {
    public class BOOKTeachesDecider : TES5.BOOKTeachesDecider {
        public override int Decide(Container container) {
            var flagsElement = container?.GetElement("Flags") as ValueElement;
            if (flagsElement == null) return 0;
            long flags = flagsElement.data?.ToInt64() ?? 0;
            if ((flags & 0x01) != 0) return 1;
            if ((flags & 0x04) != 0) return 2;
            return (flags & 0x10) != 0 ? 3 : 0;
        }
    }
    public class COEDOwnerDecider : TES5.COEDOwnerDecider { }
    public class CTDACompValueDecider : TES5.CTDACompValueDecider { }
    public class CTDAParam1Decider : TES5.CTDAParam1Decider {
        internal override int GetUnionIndex(CTDAFunctionParamType paramType) {
            return UnionIndex(paramType);
        }

        internal static int UnionIndex(CTDAFunctionParamType paramType) {
            return paramType switch {
                CTDAFunctionParamType.ptNone => 1,
                CTDAFunctionParamType.ptInteger => 3,
                CTDAFunctionParamType.ptFloat => 4,
                CTDAFunctionParamType.ptActor => 5,
                CTDAFunctionParamType.ptActorBase => 6,
                CTDAFunctionParamType.ptActorValue => 7,
                CTDAFunctionParamType.ptAdvanceAction => 8,
                CTDAFunctionParamType.ptAlias => 9,
                CTDAFunctionParamType.ptAlignment => 10,
                CTDAFunctionParamType.ptAssociationType => 11,
                CTDAFunctionParamType.ptAxis => 12,
                CTDAFunctionParamType.ptCastingSource => 13,
                CTDAFunctionParamType.ptCell => 14,
                CTDAFunctionParamType.ptClass => 15,
                CTDAFunctionParamType.ptCrimeType => 16,
                CTDAFunctionParamType.ptCriticalStage => 17,
                CTDAFunctionParamType.ptEncounterZone => 18,
                CTDAFunctionParamType.ptEquipType => 19,
                CTDAFunctionParamType.ptEvent => 20,
                CTDAFunctionParamType.ptEventData => 21,
                CTDAFunctionParamType.ptFaction => 22,
                CTDAFunctionParamType.ptFormList => 23,
                CTDAFunctionParamType.ptFormType => 24,
                CTDAFunctionParamType.ptFurniture => 25,
                CTDAFunctionParamType.ptFurnitureAnim => 26,
                CTDAFunctionParamType.ptFurnitureEntry => 27,
                CTDAFunctionParamType.ptGlobal => 28,
                CTDAFunctionParamType.ptIdleForm => 29,
                CTDAFunctionParamType.ptInventoryObject => 30,
                CTDAFunctionParamType.ptKeyword => 31,
                CTDAFunctionParamType.ptLocation => 32,
                CTDAFunctionParamType.ptMagicEffect => 33,
                CTDAFunctionParamType.ptMagicItem => 34,
                CTDAFunctionParamType.ptMiscStat => 35,
                CTDAFunctionParamType.ptObjectReference => 36,
                CTDAFunctionParamType.ptOwner => 37,
                CTDAFunctionParamType.ptPackage => 38,
                CTDAFunctionParamType.ptPackdata => 39,
                CTDAFunctionParamType.ptPerk => 40,
                CTDAFunctionParamType.ptQuest => 41,
                CTDAFunctionParamType.ptQuestStage => 42,
                CTDAFunctionParamType.ptRace => 43,
                CTDAFunctionParamType.ptReferencableObject => 44,
                CTDAFunctionParamType.ptRefType => 45,
                CTDAFunctionParamType.ptRegion => 46,
                CTDAFunctionParamType.ptScene => 47,
                CTDAFunctionParamType.ptSex => 48,
                CTDAFunctionParamType.ptShout => 49,
                CTDAFunctionParamType.ptVariableName => 50,
                CTDAFunctionParamType.ptVATSValueFunction => 51,
                CTDAFunctionParamType.ptVATSValueParam => 52,
                CTDAFunctionParamType.ptVoiceType => 53,
                CTDAFunctionParamType.ptWardState => 54,
                CTDAFunctionParamType.ptWeather => 55,
                CTDAFunctionParamType.ptWorldspace => 56,
                _ => 0
            };
        }

        protected override CTDAFunctionParamType? ResolveParamType(
            Container container, CTDAFunction ctdaFunction
        ) {
            var runOn = container?.GetElement("Run On") as ValueElement;
            if (ctdaFunction.name == "GetIsCurrentPackage" &&
                runOn != null && Convert.ToUInt32(runOn.data?.ToInt64() ?? 0) == 5 &&
                (GetParamFlags(container) & 0x02) != 0) {
                return ctdaFunction.paramType1;
            }
            return base.ResolveParamType(container, ctdaFunction);
        }
    }
    public class CTDAParam2Decider : TES5.CTDAParam2Decider {
        internal override int GetUnionIndex(CTDAFunctionParamType paramType) {
            return CTDAParam1Decider.UnionIndex(paramType);
        }
    }
    public class CTDAParam2VATSValueParamDecider : TES5.CTDAParam2VATSValueParamDecider { }
    public class CTDAReferenceDecider : TES5.CTDAReferenceDecider { }
    public class EPFDDecider : TES5.EPFDDecider { }
    public class GMSTUnionDecider : TES5.GMSTUnionDecider { }
    public class MGEFAssocItemDecider : TES5.MGEFAssocItemDecider { }
    public class NAVIIslandDataDecider : TES5.NAVIIslandDataDecider { }
    public class NPCLevelDecider : TES5.NPCLevelDecider { }
    public class NVNMParentDecider : TES5.NVNMParentDecider {
        public override int Decide(Container container) {
            var parent = container?.GetElement("Parent Worldspace")
                as ValueElement;
            if (!(parent?.data is FormId formId)) return 0;
            return formId.fileFormId == 0 ? 1 : 0;
        }
    }
    public class PerkDATADecider : TES5.PerkDATADecider { }
    public class PubPackCNAMDecider : TES5.PubPackCNAMDecider { }
    public class REFRRecordFlagsDecider : TES5.REFRRecordFlagsDecider {
        public override int Decide(Container container) {
            var baseRec = (MainRecord)container?.GetElement(@"..\@NAME");
            return UnionIndex(baseRec?.signature.ToString());
        }

        internal static int UnionIndex(string signature) {
            return signature switch {
                "ACTI" => 1,
                "STAT" => 1,
                "SCOL" => 1,
                "TREE" => 1,
                "CONT" => 2,
                "TERM" => 2,
                "DOOR" => 3,
                "LIGH" => 4,
                "MSTT" => 5,
                "ADDN" => 6,
                "SCRL" => 7,
                "AMMO" => 7,
                "ARMO" => 7,
                "BOOK" => 7,
                "INGR" => 7,
                "KEYM" => 7,
                "MISC" => 7,
                "FURN" => 7,
                "WEAP" => 7,
                "ALCH" => 7,
                _ => 0
            };
        }
    }
    public class ScriptObjFormatDecider : TES5.ScriptObjFormatDecider { }
    public class TypeDecider : TES5.TypeDecider { }
}
