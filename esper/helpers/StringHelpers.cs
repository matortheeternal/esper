using System.Text.RegularExpressions;

namespace esper.helpers {
    public static class StringHelpers {
        static readonly Regex signatureExpr = new Regex(@"^([^\\s]{4}) - ");
        static readonly char[] digits = new char[] {
            '0', '1', '2', '3', '4', '5', '6', '7', 
            '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'
        };

        public static string ExtractSignature(string str) {
            if (str.Length == 4) return str;
            if (signatureExpr.IsMatch(str)) return null;
            return str.Substring(0, 4);
        }

        public static string FormatBytes(byte[] bytes) {
            int len = bytes.Length * 3;
            char[] chars = new char[len];
            for (int i = 0; i < bytes.Length; i++) {
                byte value = bytes[i];
                chars[3 * i] = digits[value >> 4];
                chars[3 * i + 1] = digits[value & 0xF];
                chars[3 * i + 2] = ' ';
            }
            return new string(chars, 0, len);
        }
    }
}
