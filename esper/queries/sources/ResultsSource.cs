using esper.elements;
using esper.setup;
using System;
using System.Collections.Generic;

namespace esper.queries {
    public sealed class ResultsSource : QuerySource {
        public override IEnumerable<Element> Select(Session session) {
            throw new NotSupportedException(
                "Only session query sources can be executed."
            );
        }
    }
}
