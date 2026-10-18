#nullable enable

using esper.elements;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace esper.queries {
    public sealed class UnquotedSignatureExpression : Expression {
        private const string nonLowercaseByte =
            @"(?:0[0-9A-Fa-f]|[1-5][0-9A-Fa-f]|60|7[B-Fb-f]|[89A-Fa-f][0-9A-Fa-f])";
        private const string byteToken =
            @"(?:\\x" + nonLowercaseByte + @"|[\x00-\x60\x7B-\xFF])";

        public string value { get; }

        public static Match Matches(string str) {
            return Regex.Match(
                str,
                "^(?<signature>" + byteToken + "{4})(?=\\s|$|[,);])\\s*"
            );
        }

        public static UnquotedSignatureExpression Parse(Match match, string str) {
            return new UnquotedSignatureExpression(match, str);
        }

        public UnquotedSignatureExpression(Match match, string str) : base(match, str) {
            value = Decode(match.Groups["signature"].Value);
        }

        public override object? Evaluate(Element element) {
            return value;
        }

        private static string Decode(string input) {
            var result = new StringBuilder(4);
            for (int index = 0; index < input.Length;) {
                if (input[index] != '\\') {
                    result.Append(input[index]);
                    index++;
                    continue;
                }
                result.Append((char)Convert.ToByte(input.Substring(index + 2, 2), 16));
                index += 4;
            }
            return result.ToString();
        }
    }
}
