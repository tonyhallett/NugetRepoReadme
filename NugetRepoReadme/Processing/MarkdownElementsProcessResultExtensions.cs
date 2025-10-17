using System.Linq;
using Markdig.Syntax;
using NugetRepoReadme.MarkdigHelpers;

namespace NugetRepoReadme.Processing
{
    internal static class MarkdownElementsProcessResultExtensions
    {
        public static bool HasErrors(this IMarkdownElementsProcessResult markdownElementsProcessResult)
            => markdownElementsProcessResult.MissingReadmeAssets.Any() || markdownElementsProcessResult.UnsupportedImageDomains.Any();

        public static void RemoveHtmlBlock(this MarkdownElementsProcessResult markdownElementsProcessResult, HtmlBlock htmlBlock)
        {
            /*
                        SourceSpan does not include new line characters
                        Also given
                        ```
                        markdown

                        <div>div</div>
                        ```

                        The new line characters from the second line is not present in the AST at all

                    */

            // remove the new line added by the html block
            SourceSpan adjustedSpan = htmlBlock.Span.ExpandRight(htmlBlock.NewLineLength());
            markdownElementsProcessResult.AddSourceReplacement(adjustedSpan, string.Empty);
        }
    }
}
