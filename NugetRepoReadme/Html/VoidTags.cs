using System;
using System.Collections.Generic;

namespace NugetRepoReadme.Html
{
    internal static class VoidTags
    {
        public static readonly HashSet<string> Tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "area",
            "base",
            "br",
            "col",
            "embed",
            "hr",
            "img",
            "input",
            "link",
            "meta",
            "param",
            "source",
            "track",
            "wbr",
        };

        public static bool IsVoidTag(string tagName) => Tags.Contains(tagName.ToLower());
    }
}
