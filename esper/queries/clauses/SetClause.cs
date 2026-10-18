using esper.elements;
using esper.resolution;
using System;
using System.Text.RegularExpressions;

namespace esper.queries {
    public sealed class SetClause : QueryClause {
        public PropertyExpression property { get; }
        public Expression value { get; }

        public static Match Matches(string str) {
            return Regex.Match(str, @"^SET\b\s*", RegexOptions.IgnoreCase);
        }

        public static SetClause Parse(Match match, string str) {
            return new SetClause(match, str);
        }

        public SetClause(Match match, string str) : base(match, str) {
            property = ParseProperty();
            Match equals = Regex.Match(remainingStr, @"^=\s*");
            if (!equals.Success)
                throw new FormatException($"Expected '=' at: {remainingStr}");
            remainingStr = remainingStr.Substring(equals.Length);
            value = Expression.Parse(remainingStr);
            remainingStr = value.remainingStr;
        }

        public string Evaluate(Element element) {
            return QueryValue.text(value.Evaluate(element), "UPDATE");
        }

        public void Validate(Element element) {
            if (element is ValueElement && Is("value")) return;
            if (element is MainRecord record && Is("editorId")) {
                ValidateRecordProperty(record, "EDID");
                return;
            }
            if (element is MainRecord fullNameRecord && Is("fullName")) {
                ValidateRecordProperty(fullNameRecord, "FULL");
                return;
            }
            throw new InvalidOperationException(
                $"UPDATE cannot set '{property.name}' on '{element.fullPath}'."
            );
        }

        public void Apply(Element element, string newValue) {
            if (element is ValueElement valueElement) {
                valueElement.value = newValue;
                return;
            }
            if (element is MainRecord record && Is("editorId")) {
                record.editorId = newValue;
                return;
            }
            if (element is MainRecord fullNameRecord && Is("fullName")) {
                fullNameRecord.fullName = newValue;
                return;
            }
            throw new InvalidOperationException(
                $"UPDATE cannot set '{property.name}' on '{element.fullPath}'."
            );
        }

        private PropertyExpression ParseProperty() {
            Match match = PropertyExpression.Matches(remainingStr);
            if (!match.Success)
                throw new FormatException($"Expected property at: {remainingStr}");
            var parsed = PropertyExpression.Parse(match, remainingStr);
            remainingStr = parsed.remainingStr;
            return parsed;
        }

        private bool Is(string name) {
            return property.name.Equals(name, StringComparison.OrdinalIgnoreCase);
        }

        private static void ValidateRecordProperty(MainRecord record, string path) {
            if (record.GetElement(path) is ValueElement) return;
            throw new InvalidOperationException(
                $"UPDATE cannot set '{path}' on '{record.fullPath}'."
            );
        }
    }
}
