using esper.elements;
using System;

namespace esper.resolution.strategies {
    public class ResolutionStrategy {
        public virtual bool canResolve => true;

        public virtual MatchData Match(Element element, string pathPart) {
            throw new NotImplementedException();
        }

        public virtual Element Resolve(MatchData match) {
            throw new NotImplementedException();
        }

        public virtual Element Create(MatchData match) {
            throw new NotImplementedException();
        }
    }
}
