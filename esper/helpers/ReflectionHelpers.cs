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
                var info= type.GetField(propKey);
                if (info == null) continue;
                string key = (string)info.GetValue(null);
                dict[key] = type;
            }
        }
    }
}
