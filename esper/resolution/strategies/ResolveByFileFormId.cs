using esper.elements;
using esper.plugins;
using System.Globalization;

namespace esper.resolution.strategies {
    public class ResolveByFileFormId : ResolutionStrategy {
        private static readonly Regex expr = new Regex(@"^(&|0x)([0-9A-F]{1,8})$");

        public override MatchData Match(Element element, string pathPart) {
            if (element is PluginFile || element is GroupRecord)
                return FormIdMatch.From(element, pathPart, expr);
            return null;
        }

        private MainRecord ResolveRecord(FormIdMatch c) {
            var hexStr = c.match.Groups[2].Value;
            var fileFormId = UInt32.Parse(hexStr, NumberStyles.HexNumber);
            return c.target.file.GetRecordByFormId(fileFormId);
        }

        public override Element Resolve(MatchData match) {
            return ResolveRecord(match as FormIdMatch);
        }

        public override Element Create(MatchData match) {
            var c = (FormIdMatch)match;
            return c.rec?.CopyTo(c.target, 
                CopyOptions.CopyPreviousOverride    
            );
        }
    }
}
