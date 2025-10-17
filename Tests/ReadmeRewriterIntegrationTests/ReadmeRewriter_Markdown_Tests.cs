using NugetRepoReadme.NugetValidation;
using NugetRepoReadme.Processing;
using NugetRepoReadme.RemoveReplace.Settings;
using NugetRepoReadme.Rewriter;

namespace Tests.ReadmeRewriterIntegrationTests
{
    internal sealed class ReadmeRewriter_Markdown_Tests : ReadmeRewriter_Tests_Base
    {
        [TestCase("dir/fileName.ext", "username", "reponame", "main")]
        [TestCase("dir2/fileName2.ext", "username2", "reponame2", "master")]
        public void Should_Rewrite_Relative_Markdown_Image(string relativePath, string username, string reponame, string repoRef)
        {
            string readmeContent = CreateMarkdownImage(relativePath);
            string repoUrl = CreateGitHubRepositoryUrl(username, reponame);
            string expectedRedmeRewritten = CreateMarkdownImage($"https://raw.githubusercontent.com/{username}/{reponame}/{repoRef}/{relativePath}");
            string? readmeRewritten = ReadmeRewriter.Rewrite(RewriteTagsOptions.None, readmeContent, "/readme.md", repoUrl, repoRef, null, DummyReadmeRelativeFileExists)!.RewrittenReadme;
            Assert.That(readmeRewritten, Is.EqualTo(expectedRedmeRewritten));
        }

        [Test]
        public void Should_Be_Able_To_Rewrite_Multiple_At_Different_Depths_Without_Extension()
        {
            static string CreateReadme(string linkUrl) => $@"# Heading with [link]({linkUrl})

Top-level paragraph with [paragraph link]({linkUrl}).

* List item with [list link]({linkUrl})
  * Nested list item with [nested link]({linkUrl})
    * Double nested with **[bold link]({linkUrl})**

> Blockquote with [blockquote link]({linkUrl})
>
> * List inside quote with [quote list link]({linkUrl})

Paragraph with *italic [italic link]({linkUrl})* and **bold [bold link]({linkUrl})**.

1. Ordered list with [ordered link]({linkUrl})
2. Second item with
   - Nested unordered list with [mixed nesting link]({linkUrl})

> Multi-line blockquote  
> continuing with [continued link]({linkUrl})


- [ ] Top-level task with [link]({linkUrl})
- [x] Completed task with [link]({linkUrl})
- [ ] Task with nested tasks:
  - [ ] Nested task 1 with [link]({linkUrl})
  - [x] Nested task 2 with [link]({linkUrl})
    - [ ] Double-nested task with [link]({linkUrl})
---
";
            LinkTest(CreateReadme);
        }

        [Test]
        public void Should_Work_Inside_Tables_With_Extension()
        {
            static string CreateReadme(string linkUrl) => $@"
| Column A | Column B |
|-----------|-----------|
| cell with [table link]({linkUrl}) | another [table cell link]({linkUrl}) |
";
            LinkTest(CreateReadme);
        }

        private void LinkTest(Func<string, string> readMeCreator)
        {
            const string expectedAbsoluteUrl = "https://github.com/username/reponame/blob/main/relative.md";
            ReadmeRewriterResult readmeRewritten = RewriteUserRepoMainReadMe(readMeCreator("relative.md"));
            Assert.That(readmeRewritten.RewrittenReadme, Is.EqualTo(readMeCreator(expectedAbsoluteUrl)));
        }

        [Test]
        public void Should_Not_Rewrite_Relative_Markdown_Image_In_Code_Block()
        {
            string codeBlock = @$"
    ```html
    ${CreateMarkdownImage("dir/file.png")}
    ```
";
            string? readmeRewritten = RewriteUserRepoMainReadMe(codeBlock).RewrittenReadme;

            Assert.That(readmeRewritten, Is.EqualTo(codeBlock));
        }

        [Test]
        public void Should_Not_Rewrite_Relative_Markdown_Image_In_Inline_Code_Block()
        {
            string codeBlock = $"`${CreateMarkdownImage("dir/file.png")}`";
            string? readmeRewritten = RewriteUserRepoMainReadMe(codeBlock).RewrittenReadme;

            Assert.That(readmeRewritten, Is.EqualTo(codeBlock));
        }

        [Test]
        public void Should_Not_Rewrite_Relative_Markdown_Image_In_Inline_Code()
        {
            string inlineCode = $"Here is some inline code: `{CreateMarkdownImage("dir/file.png")}` in a sentence.";
            string? readmeRewritten = RewriteUserRepoMainReadMe(inlineCode).RewrittenReadme;
            Assert.That(readmeRewritten, Is.EqualTo(inlineCode));
        }

        [TestCase("https://raw.githubusercontent.com/me/repo/refs/heads/master/dir/file.gif", false)]
        [TestCase("https://untrusted/file.gif", true)]
        public void Should_Report_On_Untrusted_Image_Domains(string imageUrl, bool expectedUntrusted)
        {
            string readmeContent = CreateMarkdownImage(imageUrl);

            ReadmeRewriterResult result = RewriteUserRepoMainReadMe(readmeContent);
            IEnumerable<string> unsupportedImageDomains = result.UnsupportedImageDomains;
            if (expectedUntrusted)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(result.RewrittenReadme, Is.Null);
                    Assert.That(unsupportedImageDomains.Single(), Is.EqualTo("untrusted"));
                });
            }
            else
            {
                Assert.Multiple(() =>
                {
                    Assert.That(result.RewrittenReadme, Is.EqualTo(readmeContent));
                    Assert.That(unsupportedImageDomains, Is.Empty);
                });
            }
        }

        [Test]
        public void Should_Trust_GitHub_Badge_Urls()
        {
            const string workflowBadgeMarkdown = @"
[![Workflow name](https://github.com/user/repo/actions/workflows/workflowname.yaml/badge.svg)](https://github.com/user/repo/actions/workflows/workflowname.yaml)
";
            Assert.Multiple(() =>
            {
                Assert.That(NuGetTrustedImageDomains.Instance.IsImageDomainTrusted("github.com"), Is.False);

                Assert.That(RewriteUserRepoMainReadMe(workflowBadgeMarkdown).UnsupportedImageDomains, Is.Empty);
            });
        }

        [Test]
        public void Should_Rewrite_Reference_Image_Links()
        {
            const string readmeContent = @"
![alt][label]

[label]: image.png
";
            string? rewrittenReadMe = RewriteUserRepoMainReadMe(readmeContent).RewrittenReadme;

            const string expectedReadme = @"
![alt][label]

[label]: https://raw.githubusercontent.com/username/reponame/main/image.png
";
            Assert.That(rewrittenReadMe, Is.EqualTo(expectedReadme));
        }

        [Test]
        public void Should_Rewrite_Reference_Links()
        {
            const string readmeContent = @"
[alt][label]

[label]: page.md
";
            string? rewrittenReadMe = RewriteUserRepoMainReadMe(readmeContent).RewrittenReadme;

            const string expectedReadme = @"
[alt][label]

[label]: https://github.com/username/reponame/blob/main/page.md
";
            Assert.That(rewrittenReadMe, Is.EqualTo(expectedReadme));
        }

        [Test]
        public void Should_Replace_Readme_Marker()
        {
            const string readMeContent = @"
Intro
# Github only
";
            string githubReplacementLine = $"For full details visit [GitHub]({ReadmeRewriter.ReadmeMarker})";
            RemovalOrReplacement githubReplacement = new(CommentOrRegex.Regex, "# Github only", null, githubReplacementLine);
            var removeReplaceSettings = new RemoveReplaceSettings(null, [githubReplacement], []);

            string? rewrittenReadMe = RewriteUserRepoMainReadMe(readMeContent, RewriteTagsOptions.None, removeReplaceSettings).RewrittenReadme;

            const string expectedReadMeContent = @"
Intro
For full details visit [GitHub](https://github.com/username/reponame/blob/main/readme.md)";

            Assert.That(rewrittenReadMe, Is.EqualTo(expectedReadMeContent));
        }

        [Test]
        public void Should_Check_That_Readme_Asset_Exists()
        {
            string readmeContent = CreateMarkdownImage("/relativePath");
            string repoUrl = CreateGitHubRepositoryUrl("user", "repo");
            _ = ReadmeRewriter.Rewrite(RewriteTagsOptions.None, readmeContent, "/readme.md", repoUrl, "main", null, DummyReadmeRelativeFileExists);

            Assert.That(DummyReadmeRelativeFileExists.RelativePath, Is.EqualTo("/relativePath"));
        }

        [Test]
        public void Should_Have_MissingReadmeAssets_When_RelativeReadmeRelativeFileExists_False()
        {
            DummyReadmeRelativeFileExists.FileExists = false;
            string relativeImage = CreateMarkdownImage("/relativeImagePath");
            string relativeLink = CreateMarkdownLink("/relativeLinkPath");
            string readmeContent = $@"
{relativeImage}  
{relativeLink}
";
            string repoUrl = CreateGitHubRepositoryUrl("user", "repo");
            ReadmeRewriterResult result = ReadmeRewriter.Rewrite(RewriteTagsOptions.None, readmeContent, "/readme.md", repoUrl, "main", null, DummyReadmeRelativeFileExists);

            Assert.That(result.MissingReadmeAssets, Is.EqualTo(new List<string> { "/relativeImagePath", "/relativeLinkPath" }));
        }

        [Test]
        public void Should_Have_UnsupportedRepo_When_Null_Argument()
        {
            ReadmeRewriterResult result = ReadmeRewriter.Rewrite(RewriteTagsOptions.None, string.Empty, "/readme.md", null, "null", null, DummyReadmeRelativeFileExists);

            Assert.That(result.UnsupportedRepo, Is.True);
        }

        [Test]
        public void Should_Not_Throw_with_Comments()
        {
            const string readmeWithComment = "<!-- a comment -->";
            _ = RewriteUserRepoMainReadMe(readmeWithComment);
        }
    }
}
