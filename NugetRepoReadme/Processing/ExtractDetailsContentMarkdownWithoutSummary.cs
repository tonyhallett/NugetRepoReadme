using System.Collections.Generic;
using System.Linq;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;

namespace NugetRepoReadme.Processing
{
    internal static class ExtractDetailsContentMarkdownWithoutSummary
    {
        public static string? Extract(IHtmlDetailsElement htmlDetailsElement)
        {
            List<IText> texts = new List<IText>();
            bool detailsContainsUnsupportedHtml = false;
            foreach (INode childNode in htmlDetailsElement.ChildNodes)
            {
                // will want all text nodes but clear when encounter summary
                if (childNode is IText textNode)
                {
                    texts.Add(textNode);
                }
                else
                {
                    // can it be null
                    if (childNode is IHtmlElement htmlElement)
                    {
                        if (htmlElement.TagName.ToLower() == TagNames.Summary)
                        {
                            texts.Clear();
                        }
                        else
                        {
                            detailsContainsUnsupportedHtml = true;
                        }
                    }
                }
            }

            return !detailsContainsUnsupportedHtml
                ? string.Concat(texts.Select(t => t.Text.TrimStart('\n').TrimEnd('\n').Replace("\n", "\r\n")))
                : null;
        }
    }
}
