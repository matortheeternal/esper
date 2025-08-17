using esper.elements;
using esper.plugins;

namespace esper.data {
    public class FormIdFormat {
        public virtual string ToString(FormId fid) {
            throw new NotImplementedException();
        }

        public virtual FormId Parse(ValueElement element, string value) {
            throw new NotImplementedException();
        }
    }

    public class BraceFormat : FormIdFormat {
        private readonly static Regex braceExpr = new Regex(
            @"^\{([^:]+):([0-9A-Fa-f]{6})\}$"
        );

        public override string ToString(FormId fid) {
            if (fid.localFormId == 0) return "{Null:000000}";
            return string.Format(
                "{{{0}:{1}}}",
                fid.targetFileName,
                fid.localFormId.ToString("X6")
            );
        }

        public override FormId Parse(ValueElement element, string value) {
            var match = braceExpr.Match(value);
            if (match == null) return null;
            var filename = match.Groups[1].Value;
            var manager = element.file.session.pluginManager;
            var targetFile = filename == "Null" 
                ? null
                : manager.GetFileByName(filename);
            var ordinal = element.file.FileToOrdinal(targetFile, false);
            var localFormId = UInt32.Parse(match.Groups[2].Value);
            UInt32 fileFormId = (UInt32) ((ordinal << 24) + localFormId);
            return new FormId(targetFile, fileFormId);
        }
    }
}
