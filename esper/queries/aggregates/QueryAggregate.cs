#nullable enable

using esper.elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace esper.queries {
    public abstract class QueryAggregate : ASTNode {
        internal static readonly NodeParser<QueryAggregate>[] parsers = [
            new(CountAggregate.Matches, CountAggregate.Parse),
            new(AverageAggregate.Matches, AverageAggregate.Parse),
            new(MinAggregate.Matches, MinAggregate.Parse),
            new(MaxAggregate.Matches, MaxAggregate.Parse),
            new(SumAggregate.Matches, SumAggregate.Parse)
        ];

        private readonly string input;

        public string text { get; private set; } = "";

        protected QueryAggregate(Match match, string str) : base(match, str) {
            input = str;
        }

        public abstract object? Evaluate(IEnumerable<Element> elements);

        protected static decimal[] Numbers(
            IEnumerable<Element> elements,
            Expression expression,
            string aggregateName
        ) {
            return elements.Select(element => Number(
                expression.Evaluate(element), aggregateName
            )).ToArray();
        }

        private static decimal Number(object? value, string aggregateName) {
            if (QueryValue.TryGetNumber(value, out decimal number)) return number;
            string type = value?.GetType().Name ?? "null";
            throw new InvalidOperationException(
                $"{aggregateName} requires numeric values, not '{type}'."
            );
        }

        protected void SetText() {
            int length = input.Length - remainingStr.Length;
            text = input.Substring(0, length).Trim();
        }
    }

    public abstract class NumericAggregate : QueryAggregate {
        public Expression expression { get; }

        protected NumericAggregate(Match match, string str) : base(match, str) {
            expression = Expression.Parse(remainingStr);
            remainingStr = expression.remainingStr;
            SetText();
        }

        protected decimal[] Values(IEnumerable<Element> elements, string aggregateName) {
            return Numbers(elements, expression, aggregateName);
        }
    }
}
