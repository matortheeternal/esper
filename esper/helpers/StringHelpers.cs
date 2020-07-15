using System;
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

        private static int ParseHexChar(char c) {
            if (c >= '0' && c <= '9') return c - '0';
            if (c >= 'A' && c <= 'F') return 10 + c - 'A';
            if (c >= 'a' && c <= 'f') return 10 + c - 'a';
            throw new Exception(c + " is not a valid hexadecimal digit.");
        }

        public static byte[] ParseBytes(string value) {
            if (value.Length % 3 != 0)
                throw new ArgumentException("Input string improperly formatted.");
            byte[] bytes = new byte[value.Length / 3];
            for (int i = 0; i < bytes.Length; i++) {
                bytes[i] = (byte) (
                    ParseHexChar(value[3 * i]) << 4 + 
                    ParseHexChar(value[3 * i + 1])
                );
            }
            return bytes;
        }

        public static void SplitPath(
            string path, out string pathPart, out string remainingPath
        ) {
            int separatorIndex = path.IndexOf(@"\");
            if (separatorIndex == -1) {
                pathPart = path;
                remainingPath = "";
                return;
            }
            pathPart = path.Substring(0, separatorIndex);
            remainingPath = path.Substring(separatorIndex + 1, path.Length);
        }
    }
}
