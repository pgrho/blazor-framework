using System;
using System.Collections.Generic;
using System.Linq;

namespace Shipwreck.BlazorFramework.Components
{
    internal static class AttributeHelper
    {
        public static IEnumerable<KeyValuePair<string, object>> MergeAttributes(this IEnumerable<KeyValuePair<string, object>> element, IEnumerable<KeyValuePair<string, object>> theme)
        {
            if (element != null)
            {
                if (theme != null)
                {
                    return element.Concat(theme).GroupBy(e => e.Key).Select(e => e.First());
                }
                else
                {
                    return element;
                }
            }
            return theme ?? Enumerable.Empty<KeyValuePair<string, object>>();
        }

        public static IEnumerable<KeyValuePair<string, object>> AppendClass(this IEnumerable<KeyValuePair<string, object>> attributes, string cssClass)
        {
            if (attributes != null)
            {
                if (!string.IsNullOrEmpty(cssClass))
                {
                    return AppendClassCore(attributes, cssClass);
                }
                return attributes;
            }
            else if (!string.IsNullOrEmpty(cssClass))
            {
                return new[] { new KeyValuePair<string, object>("class", cssClass) };
            }
            return Enumerable.Empty<KeyValuePair<string, object>>(); ;
        }

        private static IEnumerable<KeyValuePair<string, object>> AppendClassCore(IEnumerable<KeyValuePair<string, object>> attributes, string cssClass)
        {
            var found = false;
            foreach (var kv in attributes)
            {
                if (!found && "class".Equals(kv.Key, StringComparison.InvariantCultureIgnoreCase))
                {
                    yield return new KeyValuePair<string, object>("class", kv.Value + " " + cssClass);
                    found = true;
                }
                else
                {
                    yield return kv;
                }
            }
            if (!found)
            {
                yield return new KeyValuePair<string, object>("class", cssClass);
            }
        }

        public static IEnumerable<KeyValuePair<string, object>> PrependStyle(this IEnumerable<KeyValuePair<string, object>> attributes, string style)
        {
            if (attributes != null)
            {
                if (!string.IsNullOrEmpty(style))
                {
                    return PrependStyleCore(attributes, style);
                }
                return attributes;
            }
            else if (!string.IsNullOrEmpty(style))
            {
                return new[] { new KeyValuePair<string, object>("style", style) };
            }
            return Enumerable.Empty<KeyValuePair<string, object>>(); ;
        }

        private static IEnumerable<KeyValuePair<string, object>> PrependStyleCore(IEnumerable<KeyValuePair<string, object>> s, string c)
        {
            var found = false;
            foreach (var kv in s)
            {
                if (!found && "style".Equals(kv.Key, StringComparison.InvariantCultureIgnoreCase))
                {
                    yield return new KeyValuePair<string, object>("style", c + ";" + kv.Value);
                    found = true;
                }
                else
                {
                    yield return kv;
                }
            }
            if (!found)
            {
                yield return new KeyValuePair<string, object>("style", c);
            }
        }
    }
}
