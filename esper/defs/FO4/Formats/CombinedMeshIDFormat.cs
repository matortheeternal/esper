using esper.elements;
using esper.setup;

namespace esper.defs.FO4 {
    public class CombinedMeshIDFormat : FormatDef {
        public static readonly string defId = "CombinedMeshIDFormat";

        internal CombinedMeshIDFormat(
            DefinitionManager manager, JObject src
        ) : base(manager, src) { }

        internal override string DataToValue(
            ValueElement element, dynamic data
        ) {
            var meshId = Convert.ToUInt32(data).ToString("X8");
            var cell = element?.record?.master;
            if (cell == null) return meshId;

            var folder = cell.file.pluginSlot?.GetOrdinal() > 0
                ? cell.file.name + @"\"
                : "";
            return $@"Precombined\{folder}{cell.localFormId:X8}_{meshId}_OC.nif";
        }

        internal override dynamic ValueToData(
            ValueElement element, string value
        ) {
            var filename = Path.GetFileName((value ?? "").Replace('\\', '/'));
            var parts = filename.Split('_');
            if (parts.Length < 3 || parts[1].Length != 8 ||
                !UInt32.TryParse(
                    parts[1],
                    System.Globalization.NumberStyles.HexNumber,
                    null,
                    out var meshId
                )) return 0u;
            return meshId;
        }
    }
}
