using Microsoft.Win32;
using System.Security;

namespace esper.helpers {
    static class RegistryWOW6432 {
        private static string GetRegKey(
            RegistryHive hive, RegistryView view,
            string keyName, string propertyName
        ) {
            if (!OperatingSystem.IsWindows()) return null;

            using var baseKey = RegistryKey.OpenBaseKey(hive, view);
            using var key = baseKey.OpenSubKey(keyName);
            return key?.GetValue(propertyName) as string;
        }

        public static string GetRegKey64(
            RegistryHive hive, string keyName, string propertyName
        ) {
            return GetRegKey(hive, RegistryView.Registry64, keyName, propertyName);
        }

        public static string GetRegKey32(
            RegistryHive hive, string keyName, string propertyName
        ) {
            return GetRegKey(hive, RegistryView.Registry32, keyName, propertyName);
        }

        // Query 64-bit, then 32-bit registry view, returning null when the
        // registry is unavailable or inaccessible.
        public static string TryGetRegKey(
            RegistryHive hive, string keyName, string propertyName
        ) {
            if (!OperatingSystem.IsWindows()) return null;
            try {
                return GetRegKey64(hive, keyName, propertyName)
                    ?? GetRegKey32(hive, keyName, propertyName);
            } catch (Exception exception) when (
                exception is IOException ||
                exception is UnauthorizedAccessException ||
                exception is SecurityException
            ) {
                return null;
            }
        }
    }
}
