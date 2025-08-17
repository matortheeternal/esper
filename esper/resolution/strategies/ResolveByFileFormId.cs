using esper.elements;
using esper.plugins;
using System.Globalization;

namespace esper.resolution.strategies {
    public class ResolveByFileFormId : ResolutionStrategy {
        private static readonly Regex expr = new Regex(@"^&([0-9A-F]{8})$");

        public override MatchData Match(Element element, string pathPart) {
            if (element is PluginFile || element is GroupRecord)
                return FormIdMatch.From(element, pathPart, expr);
            return null;
        }

        private MainRecord ResolveRecord(FormIdMatch c) {
            var hexStr = c.match.Groups[1].Value;
            var fileFormId = UInt32.Parse(hexStr, NumberStyles.HexNumber);
            return c.target.file.GetRecordByFormId(fileFormId);
        }

        /*public override Element Resolve(MatchData match) {
            var c = (FormIdMatch)match;
            c.rec = ResolveRecord(c);
            if (c.target is PluginFile file)
                return c.rec.GetInstanceInFile(file);
            if (c.target is GroupRecord group) {
                var ovr = c.rec.GetInstanceInFile(group.file);
                return ovr != null && ovr.IsContainedIn(group) ? ovr : null;
            }
            return null;
        }

        public override Element Create(MatchData match) {
            var c = (FormIdMatch)match;
            return c.rec?.CopyTo(c.target, 
                CopyOptions.CopyPreviousOverride    
            );
        }*/
    }
}
