#nullable enable

using esper.elements;
using esper.defs;
using System;

namespace esper.queries {
    internal static class QueryValue {
        private static readonly NumericReader[] numericReaders = [
            new(value => value is byte, value => (byte)value),
            new(value => value is sbyte, value => (sbyte)value),
            new(value => value is short, value => (short)value),
            new(value => value is ushort, value => (ushort)value),
            new(value => value is int, value => (int)value),
            new(value => value is uint, value => (uint)value),
            new(value => value is long, value => (long)value),
            new(value => value is ulong, value => (ulong)value),
            new(value => value is float, value => (decimal)(float)value),
            new(value => value is double, value => (decimal)(double)value),
            new(value => value is decimal, value => (decimal)value)
        ];

        internal static bool equal(object? left, object? right) {
            left = UnwrapElement(left);
            right = UnwrapElement(right);
            if (left == null || right == null) return left == right;
            if (tryNumeric(left, out decimal leftNumber)
                && tryNumeric(right, out decimal rightNumber))
                return leftNumber == rightNumber;
            if (left is string leftString && right is string rightString)
                return leftString.Equals(
                    rightString,
                    StringComparison.OrdinalIgnoreCase
                );
            return left.Equals(right);
        }

        internal static int Compare(object? left, object? right) {
            left = UnwrapElement(left);
            right = UnwrapElement(right);
            if (left == null) return right == null ? 0 : -1;
            if (right == null) return 1;
            if (left is Element) return -1;
            if (right is Element) return 1;
            if (tryNumeric(left, out decimal leftNumber)
                && tryNumeric(right, out decimal rightNumber))
                return leftNumber.CompareTo(rightNumber);
            if (left is string leftString && right is string rightString)
                return StringComparer.OrdinalIgnoreCase.Compare(
                    leftString,
                    rightString
                );
            if (left is bool leftBool && right is bool rightBool)
                return leftBool.CompareTo(rightBool);
            return -1;
        }

        internal static object? Unwrap(object? value) {
            return UnwrapElement(value);
        }

        internal static bool TryGetNumber(object? value, out decimal number) {
            if (value == null) {
                number = 0;
                return false;
            }
            return tryNumeric(value, out number);
        }

        private static object? UnwrapElement(object? value) {
            if (value is ValueElement valueElement
                && valueElement.data is VariantData data
                && IsNumeric(data))
                return data.ToPrimitive();
            if (value is ValueElement textValue) return textValue.value;
            return value;
        }

        private static bool IsNumeric(VariantData data) {
            return data is VariantData.I8
                or VariantData.U8
                or VariantData.I16
                or VariantData.U16
                or VariantData.I32
                or VariantData.U32
                or VariantData.I64
                or VariantData.U64
                or VariantData.F32;
        }

        internal static bool condition(object? value) {
            if (value is bool condition) return condition;
            string type = value?.GetType().Name ?? "null";
            throw new InvalidOperationException(
                $"A query condition must evaluate to Boolean, not '{type}'."
            );
        }

        internal static string text(object? value, string operation) {
            if (value is string text) return text;
            string type = value?.GetType().Name ?? "null";
            throw new InvalidOperationException(
                $"{operation} requires text, not '{type}'."
            );
        }

        private static bool tryNumeric(object value, out decimal number) {
            foreach (NumericReader reader in numericReaders) {
                if (!reader.Matches(value)) continue;
                number = reader.read(value);
                return true;
            }
            number = 0;
            return false;
        }

        private sealed class NumericReader {
            internal Func<object, bool> Matches { get; }
            internal Func<object, decimal> read { get; }

            internal NumericReader(
                Func<object, bool> Matches,
                Func<object, decimal> read
            ) {
                this.Matches = Matches;
                this.read = read;
            }
        }
    }
}
