using System.IO;
using Ganss.Xss;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Extensions.EmphasisExtras;
using Markdig.Renderers;
using NUnit.Framework;

namespace NugetTests
{
    internal class NugetProcessingRemoves_Test
    {
        // (https://github.com/NuGet/NuGetGallery/blob/53fe20adeb6195cb57ddc1d586ba5a3bac5cf3c1/src/NuGetGallery/Services/MarkdownService.cs#L212
        [Test]
        public void Should_Have_Literal_Html()
        {
            string markdown = @"
# title 1

<detail>

<summary>Html removed</summary>

</detail>
";
            string expectedMarkdown = "<h1 id=\"title-1\">title 1</h1>\n<p>&lt;detail&gt;</p>\n<p>&lt;summary&gt;Html removed&lt;/summary&gt;</p>\n<p>&lt;/detail&gt;</p>";
            Assert.That(NugetMarkdownParse(markdown), Is.EqualTo(expectedMarkdown));
        }

        private string NugetMarkdownParse(string markdown)
        {
            MarkdownPipeline pipeline = new MarkdownPipelineBuilder()
            .UseGridTables()
            .UsePipeTables()
            .UseListExtras()
            .UseTaskLists()
            .UseEmojiAndSmiley()
            .UseAutoLinks()
            .UseAlertBlocks()
            .UseReferralLinks("noopener noreferrer nofollow")
            .UseAutoIdentifiers(AutoIdentifierOptions.GitHub)
            .UseEmphasisExtras(EmphasisExtraOptions.Strikethrough)
            .DisableHtml() //block inline html
            .UseBootstrap()
            .Build();

            using (var htmlWriter = new StringWriter())
            {
                var renderer = new HtmlRenderer(htmlWriter);
                pipeline.Setup(renderer);

                Markdig.Syntax.MarkdownDocument document = Markdown.Parse(markdown, pipeline);

                _ = renderer.Render(document);
                string renderered = htmlWriter.ToString().Trim();

                var htmlSanitizer = new HtmlSanitizer();
                SanitizerSettings(htmlSanitizer);
                return SanitizeText(htmlSanitizer, renderered);
            }
        }

        private static void SanitizerSettings(IHtmlSanitizer htmlSanitizer)
        {
            //Configure allowed tags, attributes for the sanitizer
            _ = htmlSanitizer.AllowedAttributes.Add("id");
            _ = htmlSanitizer.AllowedAttributes.Add("class");
        }

        private string SanitizeText(IHtmlSanitizer _htmlSanitizer, string input)
            => !string.IsNullOrWhiteSpace(input) ? _htmlSanitizer.Sanitize(input) : input;
    }
}
