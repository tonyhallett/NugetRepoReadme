using NugetRepoReadme.Processing;
using NugetRepoReadme.Rewriter;

namespace Tests.ReadmeRewriterIntegrationTests
{
    internal sealed class ReadmeRewriter_Html_Tests : ReadmeRewriter_Tests_Base
    {
        [TestCase(RewriteTagsOptions.RewriteAll, true, true)]
        [TestCase(RewriteTagsOptions.RewriteImgTagsForSupportedDomains, true, false)]
        [TestCase(RewriteTagsOptions.None, false, false)]
        public void Should_Rewrite_Img_When_RewriteTagsOptions_RewriteImgTagsForSupportedDomains(RewriteTagsOptions rewriteTagsOptions, bool expectsRewrites, bool lowercaseTag)
        {
            string readmeContent = CreateImage("alttext", "https://github.com/user/repo/actions/workflows/workflowname.yaml/badge.svg", lowercaseTag);
            ReadmeRewriterResult result = RewriteUserRepoMainReadMe(readmeContent, rewriteTagsOptions);

            string expectedRewrittenReadme = CreateMarkdownImage("https://github.com/user/repo/actions/workflows/workflowname.yaml/badge.svg", "alttext");
            string expectedReadme = expectsRewrites ? expectedRewrittenReadme : readmeContent;
            Assert.Multiple(() =>
            {
                Assert.That(result.HasUnsupportedHTML, Is.False);
                Assert.That(expectedReadme, Is.EqualTo(result.RewrittenReadme));
                Assert.That(result.UnsupportedImageDomains, Is.Empty);
            });
        }

        [Test]
        public void Should_Rewrite_Relative_Img()
        {
            string readmeContent = CreateImage("alttext", "relative.png");
            ReadmeRewriterResult result = RewriteUserRepoMainReadMe(readmeContent);

            string expectedReadme = CreateMarkdownImage("https://raw.githubusercontent.com/username/reponame/main/relative.png", "alttext");
            Assert.Multiple(() =>
            {
                Assert.That(result.HasUnsupportedHTML, Is.False);
                Assert.That(expectedReadme, Is.EqualTo(result.RewrittenReadme));
                Assert.That(result.UnsupportedImageDomains, Is.Empty);
            });
        }

        [Test]
        public void Should_Not_Rewrite_Imgs_For_Unsupported_Domains()
        {
            string unsupportedImage1 = CreateImage("altext", "https://unsupported.com/a.png");
            string unsupportedImage2 = CreateImage("altext", "https://unsupported2.com/a.png");
            string readmeContent = @$"
{unsupportedImage1}

{unsupportedImage2}
";

            ReadmeRewriterResult result = RewriteUserRepoMainReadMe(readmeContent);

            Assert.Multiple(() =>
            {
                Assert.That(result.HasUnsupportedHTML, Is.False);
                Assert.That(result.RewrittenReadme, Is.Null);
                Assert.That(result.UnsupportedImageDomains, Is.EqualTo(new List<string> { "unsupported.com", "unsupported2.com" }));
            });
        }

        [TestCase(RewriteTagsOptions.RewriteAll, true)]
        [TestCase(RewriteTagsOptions.RewriteBrTags, true)]
        [TestCase(RewriteTagsOptions.None, false)]
        public void Should_Rewrite_Br_When_RewriteTagsOptions_RewriteBrTags(RewriteTagsOptions rewriteTagsOptions, bool expectsRewrites)
        {
            const string readmeContent = @"
Line1<br/>
Line2<br/>";
            ReadmeRewriterResult result = RewriteUserRepoMainReadMe(readmeContent, rewriteTagsOptions);

            const string expectedRewrittenReadme = @"
Line1\
Line2\";
            string expectedReadme = expectsRewrites ? expectedRewrittenReadme : readmeContent;

            Assert.Multiple(() =>
            {
                Assert.That(result.HasUnsupportedHTML, Is.False);
                Assert.That(result.RewrittenReadme, Is.EqualTo(expectedReadme));
            });
        }

        [TestCase("<br>")]
        [TestCase("<br />")]
        [TestCase("<BR/>")]
        public void Should_Rewrite_Br_Different_Formats(string br)
        {
            string readmeContent = $"Line1{br}";
            ReadmeRewriterResult result = RewriteUserRepoMainReadMe(readmeContent, RewriteTagsOptions.RewriteAll);

            const string expectedReadme = "Line1\\";

            Assert.Multiple(() =>
            {
                Assert.That(result.RewrittenReadme, Is.EqualTo(expectedReadme));
                Assert.That(result.HasUnsupportedHTML, Is.False);
                Assert.That(result.RewrittenReadme, Is.EqualTo(expectedReadme));
            });
        }

        [TestCase(RewriteTagsOptions.RewriteAll, true, true)]
        [TestCase(RewriteTagsOptions.RewriteATags, true, false)]
        [TestCase(RewriteTagsOptions.None, false, true)]
        public void Should_Rewrite_A_Tag_When_RewriteTagsOptions_RewriteATags_Relative(RewriteTagsOptions rewriteTagsOptions, bool expectsRewrites, bool lowercaseTag)
        {
            string aTag = lowercaseTag ? "a" : "A";
            string readmeContent = @$"<{aTag} href=""abc.html"">TextContent</{aTag}>";

            ReadmeRewriterResult result = RewriteUserRepoMainReadMe(readmeContent, rewriteTagsOptions);

            const string expectedRewrittenReadme = "[TextContent](https://github.com/username/reponame/blob/main/abc.html)";
            string expectedReadme = expectsRewrites ? expectedRewrittenReadme : readmeContent;
            Assert.Multiple(() =>
            {
                Assert.That(result.HasUnsupportedHTML, Is.False);
                Assert.That(expectedReadme, Is.EqualTo(result.RewrittenReadme));
            });
        }

        [Test]
        public void Should_Rewrite_A_Tag_Absolute()
        {
            const string readmeContent = @"<a href=""https://example.org/some-page"">Some page</a>";

            ReadmeRewriterResult result = RewriteUserRepoMainReadMe(readmeContent);

            const string expectedReadme = "[Some page](https://example.org/some-page)";
            Assert.Multiple(() =>
            {
                Assert.That(result.RewrittenReadme, Is.EqualTo(expectedReadme));
                Assert.That(result.UnsupportedImageDomains, Is.Empty);
                Assert.That(result.HasUnsupportedHTML, Is.False);
            });
        }

        [Test]
        public void Should_Have_UnsupportedHTML_When_RewriteTagsOptions_Error_And_HTML_In_Readme()
        {
            ReadmeRewriterResult result = RewriteUserRepoMainReadMe("<br/>", RewriteTagsOptions.ErrorOnHtml);
            Assert.That(result.HasUnsupportedHTML, Is.True);
        }

        [Test]
        public void Should_ExtractDetailsContentWithoutSummary()
        {
            const string readmeContent =
@"markdown

<details>
<summary>Details summary</summary>
md 1  
md 2
</details>";

            ReadmeRewriterResult result = RewriteUserRepoMainReadMe(readmeContent, RewriteTagsOptions.ExtractDetailsContentWithoutSummary);
            const string expectedReadme =
@"markdown

md 1  
md 2";
            Assert.Multiple(() =>
            {
                Assert.That(result.HasUnsupportedHTML, Is.False);
                Assert.That(result.RewrittenReadme, Is.EqualTo(expectedReadme));
            });
        }

        [Test]
        public void Should_ExtractDetailsContentWithoutSummary_Processing_A_And_Img_Tags()
        {
            string rewrittenImage = CreateMarkdownImage("https://raw.githubusercontent.com/username/reponame/main/relative.png", "alttext");

            string readmeContent =
$@"markdown

<details>
<summary>Details summary</summary>
{CreateMarkdownImage("relative.png", "alttext")}
</details>";

            ReadmeRewriterResult result = RewriteUserRepoMainReadMe(readmeContent, RewriteTagsOptions.ExtractDetailsContentWithoutSummary);
            string expectedReadme =
$@"markdown

{rewrittenImage}";

            Assert.Multiple(() =>
            {
                Assert.That(result.HasUnsupportedHTML, Is.False);
                Assert.That(result.RewrittenReadme, Is.EqualTo(expectedReadme));
            });
        }

        [Test]
        public void Should_Remove_HtmlBlock_When_RewriteTagsOptions_RemoveHtml()
        {
            const string readmeContent =
@"markdown

<div>some div</div>

more markdown";
            ReadmeRewriterResult result = RewriteUserRepoMainReadMe(readmeContent, RewriteTagsOptions.RemoveHtml);
            const string expectedReadme =
@"markdown


more markdown";

            Assert.Multiple(() =>
            {
                Assert.That(result.HasUnsupportedHTML, Is.False);
                Assert.That(result.RewrittenReadme, Is.EqualTo(expectedReadme));
            });
        }

        [TestCase("\n")]
        [TestCase("\r")]
        [TestCase("\r\n")]
        public void Should_Use_NewLine_Length_When_Removing_HtmlBlock(string newLine)
        {
            string readmeContent =
$@"markdown

<div>some div</div>{newLine}
more markdown";
            ReadmeRewriterResult result = RewriteUserRepoMainReadMe(readmeContent, RewriteTagsOptions.RemoveHtml);
            const string expectedReadme =
@"markdown


more markdown";

            Assert.Multiple(() =>
            {
                Assert.That(result.HasUnsupportedHTML, Is.False);
                Assert.That(result.RewrittenReadme, Is.EqualTo(expectedReadme));
            });
        }

        [Test]
        public void Should_Remove_HtmlInline_When_RewriteTagsOptions_RemoveHtml()
        {
            const string readmeContent =
"""
markdown

<a href="https://www.bbc.com">bbc</a>

more markdown
""";
            ReadmeRewriterResult result = RewriteUserRepoMainReadMe(readmeContent, RewriteTagsOptions.RemoveHtml);
            const string expectedReadme =
                """
markdown


more markdown
""";

            Assert.Multiple(() =>
            {
                Assert.That(result.HasUnsupportedHTML, Is.False);
                Assert.That(result.RewrittenReadme, Is.EqualTo(expectedReadme));
            });
        }

        [Test]
        public void Should_Remove_HtmlInline_Not_End_Of_Line_When_RewriteTagsOptions_RemoveHtml()
        {
            const string readmeContent =
@"<a href=""https://www.bbc.com"">bbc</a>  markdown

more markdown";
            ReadmeRewriterResult result = RewriteUserRepoMainReadMe(readmeContent, RewriteTagsOptions.RemoveHtml);
            const string expectedReadme =
@"  markdown

more markdown";

            Assert.Multiple(() =>
            {
                Assert.That(result.HasUnsupportedHTML, Is.False);
                Assert.That(result.RewrittenReadme, Is.EqualTo(expectedReadme));
            });
        }

        [Test]
        public void Should_Remove_Void_Tags()
        {
            const string readmeContent =
"""
markdown

<img src="https://www.bbc.com/logo.png" alt="bbc logo" />

more markdown
""";
            ReadmeRewriterResult result = RewriteUserRepoMainReadMe(readmeContent, RewriteTagsOptions.RemoveHtml);
            const string expectedReadme =
                """
markdown


more markdown
""";

            Assert.Multiple(() =>
            {
                Assert.That(result.HasUnsupportedHTML, Is.False);
                Assert.That(result.RewrittenReadme, Is.EqualTo(expectedReadme));
            });
        }
    }
}
