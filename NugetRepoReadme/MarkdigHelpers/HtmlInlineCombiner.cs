using System.Collections.Generic;
using Markdig.Syntax.Inlines;
using NugetRepoReadme.Html;

namespace NugetRepoReadme.MarkdigHelpers
{
    internal static class HtmlInlineCombiner
    {
        internal class HtmlInlineStartEnd
        {
            public HtmlInlineStartEnd(HtmlInline start, HtmlInline? end)
            {
                Start = start;
                End = end;
            }

            // Deconstructor
            public void Deconstruct(out HtmlInline start, out HtmlInline? end)
            {
                start = Start;
                end = End;
            }

            public HtmlInline Start { get; }

            public HtmlInline? End { get; }
        }

        private class StartInfo
        {
            public StartInfo(string tagName, HtmlInline htmlInline)
            {
                TagName = tagName;
                HtmlInline = htmlInline;
            }

            public string TagName { get; }

            public HtmlInline HtmlInline { get; }
        }

        public static List<HtmlInlineStartEnd>? Combine(IEnumerable<HtmlInline> htmlInlines)
        {
            List<HtmlInlineStartEnd> combined = new List<HtmlInlineStartEnd>();

            StartInfo? startInfo = null;
            foreach (HtmlInline htmlInline in htmlInlines)
            {
                HtmlTagInfo? tagInfo = htmlInline.GetHtmlTagInfo();
                if (tagInfo == null)
                {
                    return null;
                }

                string tagName = tagInfo.TagName;
                if (VoidTags.IsVoidTag(tagName))
                {
                    combined.Add(new HtmlInlineStartEnd(htmlInline, null));
                }
                else
                {
                    if (tagInfo.IsStart)
                    {
                        if (startInfo != null)
                        {
                            return null;
                        }

                        startInfo = new StartInfo(tagInfo.TagName, htmlInline);
                    }
                    else
                    {
                        if (tagName != startInfo?.TagName)
                        {
                            return null;
                        }

                        combined.Add(new HtmlInlineStartEnd(startInfo.HtmlInline, htmlInline));
                        startInfo = null;
                    }
                }
            }

            return startInfo != null ? null : combined;
        }
    }
}
