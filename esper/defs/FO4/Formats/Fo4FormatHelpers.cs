using esper.data;
using esper.elements;
using esper.plugins;
using esper.resolution;
using System;
using System.Globalization;

namespace esper.defs.FO4 {
    internal static class Fo4FormatHelpers {
        internal static uint WbHexStrToInt(string value) {
            value ??= "";
            var space = value.IndexOf(' ');
            var colon = value.IndexOf(':');
            var cut = space >= 0 && colon >= 0
                ? Math.Min(space, colon)
                : Math.Max(space, colon);
            var head = cut >= 0 ? value.Substring(0, cut) : value;
            return uint.TryParse(
                head,
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture,
                out var result
            ) ? result : 0u;
        }

        internal static string SafeName(Container entry, string field) {
            var ve = entry.GetElement(field) as ValueElement;
            if (ve == null) return "";
            if (ve.data is LocalizedString &&
                ve.file?.stringFiles == null)
                return "";
            return entry.GetValue(field) ?? "";
        }
    }
}
