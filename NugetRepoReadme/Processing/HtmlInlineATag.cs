using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using NugetRepoReadme.MarkdigHelpers;

namespace NugetRepoReadme.Processing
{
    internal class HtmlInlineATag
    {
        public HtmlInlineATag(string text, SourceSpan span)
        {
            Text = text;
            Span = span;
        }

        public string Text { get; }

        public SourceSpan Span { get; }

        public static HtmlInlineATag? TryCreate(HtmlInline htmlInline)
        {
            if (!IsATagStart(htmlInline))
            {
                return null;
            }

            string fullText = htmlInline.Tag;
            if (!(htmlInline.NextSibling is LiteralInline literalInline) || !(literalInline.NextSibling is HtmlInline closingATag) || !IsATagEnd(closingATag))
            {
                return null;
            }

            fullText += literalInline.Content;
            fullText += closingATag.Tag;
            SourceSpan span = htmlInline.Span.Combine(literalInline.Span, closingATag.Span);
            return new HtmlInlineATag(fullText, span);
        }

        private static bool IsATagStart(HtmlInline htmlInline)
        {
            string tag = htmlInline.Tag.Trim().ToLower();
            return tag.StartsWith("<a");
        }

        private static bool IsATagEnd(HtmlInline htmlInline)
        {
            string tag = htmlInline.Tag.Trim().ToLower();
            return tag == "</a>";
        }
    }
}
