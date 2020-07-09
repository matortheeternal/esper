using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace esper.helpers {
    public static class ReflectionHelpers {
        public static IEnumerable<Type> GetClasses(Func<Type, bool> typeFilter) {
            return Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(typeFilter);
        }

        public static void BuildClassDictionary(
            Dictionary<string, Type> dict, 
            string propKey, 
            Func<Type, bool> typeFilter
        ) {
            var types = GetClasses(typeFilter);
            foreach (var type in types) {
                PropertyInfo pInfo = type.GetProperty(propKey);
                if (pInfo == null) continue;
                string key = (string)pInfo.GetValue(null);
                dict[key] = type;
            }
        }
    }
}
