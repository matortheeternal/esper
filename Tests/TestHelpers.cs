using System;
using System.IO;

namespace Tests {
    public static class TestHelpers {
        public static string fixturesPath = Path.Combine(
            Environment.CurrentDirectory, 
            "fixtures"
        );

        public static string FixturePath(string filename) {
            return Path.Combine(fixturesPath, filename);
        }
    }
}
