#nullable enable

using esper.elements;
using System;

namespace esper.queries {
    internal static class ElementProperties {
        private static readonly ElementProperty[] properties = [
            new("name", _ => true, element => element.name),
            new("displayName", _ => true, element => element.displayName),
            new("signature", _ => true, element => element.signature.ToString()),
            new("path", _ => true, element => element.path),
            new("fullPath", _ => true, element => element.fullPath),
            new("index", _ => true, element => element.index),
            new("size", _ => true, element => element.size),
            new("file", _ => true, element => element.file.name),
            new("record", _ => true, element => element.record),
            new("value", element => element is ValueElement,
                element => ((ValueElement)element).value),
            new("editorId", element => element is MainRecord,
                element => ((MainRecord)element).editorId),
            new("fullName", element => element is MainRecord,
                element => ((MainRecord)element).fullName),
            new("formId", element => element is MainRecord,
                element => ((MainRecord)element).formId.ToString()),
            new("fileFormId", element => element is MainRecord,
                element => ((MainRecord)element).fileFormId),
            new("localFormId", element => element is MainRecord,
                element => ((MainRecord)element).localFormId),
            new("globalFormId", element => element is MainRecord,
                element => ((MainRecord)element).globalFormId),
            new("local", element => element is MainRecord,
                element => ((MainRecord)element).local),
            new("compressed", element => element is MainRecord,
                element => ((MainRecord)element).compressed),
            new("dataSize", element => element is MainRecord,
                element => ((MainRecord)element).dataSize),
            new("count", element => element is Container,
                element => ((Container)element).count)
        ];

        internal static bool tryRead(
            string name,
            Element element,
            out object? value
        ) {
            foreach (ElementProperty property in properties) {
                if (!property.name.Equals(
                    name,
                    StringComparison.OrdinalIgnoreCase
                )) continue;
                if (!property.applies(element)) continue;
                value = property.read(element);
                return true;
            }
            value = null;
            return false;
        }

        private sealed class ElementProperty {
            internal string name { get; }
            internal Func<Element, bool> applies { get; }
            internal Func<Element, object?> read { get; }

            internal ElementProperty(
                string name,
                Func<Element, bool> applies,
                Func<Element, object?> read
            ) {
                this.name = name;
                this.applies = applies;
                this.read = read;
            }
        }
    }
}
