using esper.elements;
using esper.plugins;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace esper.resolution.strategies {
    public class ResolveByGlobalFormId : ResolutionStrategy {
        private static readonly Regex expr = new Regex(@"^[0-9A-F]{8}$");

        private bool IsSupportedElement(Element element) {
            return element is RootElement || 
                   element is PluginFile  ||
                   element is GroupRecord;
        }

        public override MatchData Match(Element element, string pathPart) {
            if (IsSupportedElement(element))
                return FormIdMatch.From(element, pathPart, expr);
            return null;
        }

        private MainRecord ResolveRecord(FormIdMatch c) {
            var hexStr = c.match.Value;
            var globalFormId = UInt32.Parse(hexStr, NumberStyles.HexNumber);
            var targetIsRoot = c.target is RootElement;
            var root = (RootElement)(targetIsRoot ? c.target : c.target.file.container);
            return root.GetRecordByGlobalFormId(globalFormId);
        }

        /*public override Element Resolve(MatchData match) {
            var c = (FormIdMatch)match;
            c.rec = ResolveRecord(c);
            if (c.target is RootElement) return c.rec;
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
