using System.Text.RegularExpressions;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace NugetRepoReadme.MarkdigHelpers
{
    internal static class HtmlInlineExtensions
    {
        public static bool IsBrTag(this HtmlInline htmlInline)
        {
            string tag = htmlInline.Tag.Trim();

            // Match <br>, <br/>, <br />, <br    />, etc.
            return Regex.IsMatch(tag, @"^<br\s*/?>$", RegexOptions.IgnoreCase);
        }

        public static HtmlTagInfo? GetHtmlTagInfo(this HtmlInline inline)
        {
            string text = inline.Tag; // The raw HTML content

            // Match a start or end tag
            Match m = Regex.Match(text, @"^<\s*(/)?\s*([a-zA-Z0-9]+)");
            if (!m.Success)
            {
                return null;
            }

            bool isEnd = m.Groups[1].Success;        // '/' present → end tag
            string tagName = m.Groups[2].Value;      // tag name

            return new HtmlTagInfo(tagName.ToLower(), !isEnd);
        }

        public static int RemovalNewLineLength(this HtmlInline endHtmlInline)
        {
            int removalNewLineLength = 0;
            ContainerInline? parent = endHtmlInline.Parent;
            if (parent?.LastChild == endHtmlInline && parent.ParentBlock is LeafBlock parentBlock)
            {
                removalNewLineLength = parentBlock.NewLineLength();
            }

            return removalNewLineLength;
        }
    }
}
