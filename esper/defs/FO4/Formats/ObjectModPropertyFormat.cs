using esper.data;
using esper.elements;
using esper.helpers;
using esper.resolution;
using esper.setup;

namespace esper.defs.FO4 {
    public class ObjectModPropertyFormat : FormatDef {
        public static readonly string defId = "ObjectModPropertyFormat";

        private static readonly Signature ARMO = Signature.FromString("ARMO");
        private static readonly Signature WEAP = Signature.FromString("WEAP");
        private static readonly Signature NPC_ = Signature.FromString("NPC_");
        private static readonly Signature OMOD = Signature.FromString("OMOD");

        private readonly EnumDef armorPropertyEnum;
        private readonly EnumDef weaponPropertyEnum;
        private readonly EnumDef actorPropertyEnum;

        internal ObjectModPropertyFormat(
            DefinitionManager manager, JObject src
        ) : base(manager, src) {
            armorPropertyEnum = (EnumDef)manager.ResolveDef("ArmorPropertyEnum");
            weaponPropertyEnum = (EnumDef)manager.ResolveDef("WeaponPropertyEnum");
            actorPropertyEnum = (EnumDef)manager.ResolveDef("ActorPropertyEnum");
        }

        private EnumDef GetPropertyEnum(ValueElement element) {
            var record = element?.record;
            if (record == null) return null;
            var signature = record.signature;
            if (signature == OMOD) {
                var formType = record.GetElement(@"DATA\Form Type")
                    as ValueElement;
                if (!(formType?.data is UInt32 value)) return null;
                signature = new Signature(unchecked((int)value));
            }
            if (signature == ARMO) return armorPropertyEnum;
            if (signature == WEAP) return weaponPropertyEnum;
            if (signature == NPC_) return actorPropertyEnum;
            return null;
        }

        internal override string DataToValue(
            ValueElement element, dynamic data
        ) {
            var propertyEnum = GetPropertyEnum(element);
            return propertyEnum == null
                ? data.ToString()
                : propertyEnum.DataToValue(element, data);
        }

        internal override dynamic ValueToData(
            ValueElement element, string value
        ) {
            var propertyEnum = GetPropertyEnum(element);
            return propertyEnum == null
                ? DataHelpers.ParseInt64(value)
                : propertyEnum.ValueToData(element, value);
        }
    }
}
