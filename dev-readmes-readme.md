# Visual Studio and Nuget use Markdig

<details>

<summary>Markdig</summary>

[Markdig default pipeline](https://github.com/xoofx/markdig/blob/781d9b536598e0b770c1eabbe58d2ac76b285409/src/Markdig/MarkdownPipelineBuilder.cs#L24)

```csharp
   public MarkdownPipelineBuilder()
   {
       // Add all default parsers
       BlockParsers =
       [
           new ThematicBreakParser(),
           new HeadingBlockParser(),
           new QuoteBlockParser(),
           new ListBlockParser(),

           new HtmlBlockParser(),
           new FencedCodeBlockParser(),
           new IndentedCodeBlockParser(),
           new ParagraphBlockParser(),
       ];

       InlineParsers =
       [
           new HtmlEntityParser(),
           new LinkInlineParser(),
           new EscapeInlineParser(),
           new EmphasisInlineParser(),
           new CodeInlineParser(),
           new AutolinkInlineParser(),
           new LineBreakInlineParser(),
       ];

       Extensions = new OrderedList<IMarkdownExtension>();
   }
```

[Markdig "Use"](https://github.com/xoofx/markdig/blob/781d9b536598e0b770c1eabbe58d2ac76b285409/src/Markdig/MarkdownExtensions.cs#L76)

</details>
<details>

<summary>Visual Studio</summary>

Visual Studio. Uses Markdig.Signed 0.30.0.0

`MarkdownParserProvider`

```csharp
    internal static MarkdownPipeline GetPipeline() => new MarkdownPipelineBuilder()
      .UseAdvancedExtensions()
      .UsePragmaLines()
      .UsePreciseSourceLocation()
      .UseYamlFrontMatter()
      .UseEmojiAndSmiley()
      .UseEmphasisExtras()
      .UseTaskLists()
      .Build();
```

Note that the latest version of Markdig has added UseAlertBlocks to advanced extensions.

```csharp
        public static MarkdownPipelineBuilder UseAdvancedExtensions(this MarkdownPipelineBuilder pipeline)
       {
           return pipeline
               .UseAbbreviations()
               .UseAutoIdentifiers()
               .UseCitations()
               .UseCustomContainers()
               .UseDefinitionLists()
               .UseEmphasisExtras()
               .UseFigures()
               .UseFooters()
               .UseFootnotes()
               .UseGridTables()
               .UseMathematics()
               .UseMediaLinks()
               .UsePipeTables()
               .UseListExtras()
               .UseTaskLists()
               .UseDiagrams()
               .UseAutoLinks()
               .UseGenericAttributes(); // Must be last as it is one parser that is modifying other parsers
       }
```

</details>

<details>

<summary>Nuget</summary>

[ImageDomainValidator](https://github.com/NuGet/NuGetGallery/blob/53fe20adeb6195cb57ddc1d586ba5a3bac5cf3c1/src/NuGetGallery/Services/ImageDomainValidator.cs#L20)
[Nuget Gallery - MarkdownService](https://github.com/NuGet/NuGetGallery/blob/53fe20adeb6195cb57ddc1d586ba5a3bac5cf3c1/src/NuGetGallery/Services/MarkdownService.cs#L212)

```csharp
        private RenderedMarkdownResult GetHtmlFromMarkdownMarkdig(string markdownString, int incrementHeadersBy)
       {
           var output = new RenderedMarkdownResult()
           {
               ImagesRewritten = false,
               Content = "",
               ImageSourceDisallowed = false,
               IsMarkdigMdSyntaxHighlightEnabled = false
           };

           var markdownWithoutComments = HtmlCommentPattern.Replace(markdownString, "");

           var markdownWithImageAlt = ImageTextPattern.Replace(markdownWithoutComments, $"![{AltTextForImage}](");

           var markdownWithoutBom = markdownWithImageAlt.TrimStart('\ufeff');

           var pipeline = new MarkdownPipelineBuilder()
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

               var document = Markdown.Parse(markdownWithoutBom, pipeline);

               foreach (var node in document.Descendants())
               {
                   if (node is Markdig.Syntax.Block)
                   {
                       // Demote heading tags so they don't overpower expander headings.
                       if (node is HeadingBlock heading)
                       {
                           heading.Level = Math.Min(heading.Level + incrementHeadersBy, 6);
                       }
                   }
                   else if (node is Markdig.Syntax.Inlines.Inline)
                   {
                       if (node is LinkInline linkInline)
                       {
                           if (linkInline.IsImage)
                           {
                               if (_features.IsImageAllowlistEnabled())
                               {
                                   if (!_imageDomainValidator.TryPrepareImageUrlForRendering(linkInline.Url, out string readyUriString))
                                   {
                                       linkInline.Url = string.Empty;
                                       output.ImageSourceDisallowed = true;
                                   }
                                   else
                                   {
                                       output.ImagesRewritten = output.ImagesRewritten || (linkInline.Url != readyUriString);
                                       linkInline.Url = readyUriString;
                                   }
                               }
                               else
                               {
                                   if (!PackageHelper.TryPrepareUrlForRendering(linkInline.Url, out string readyUriString, rewriteAllHttp: true))
                                   {
                                       linkInline.Url = string.Empty;
                                   }
                                   else
                                   {
                                       output.ImagesRewritten = output.ImagesRewritten || (linkInline.Url != readyUriString);
                                       linkInline.Url = readyUriString;
                                   }
                               }
                           }
                           else
                           {
                               // Allow only http or https links in markdown. Transform link to https for known domains.
                               if (!PackageHelper.TryPrepareUrlForRendering(linkInline.Url, out string readyUriString))
                               {
                                   if (linkInline.Url != null && !linkInline.Url.StartsWith("#")) //allow internal section links
                                   {
                                       linkInline.Url = string.Empty;
                                   }
                               }
                               else
                               {
                                   linkInline.Url = readyUriString;
                               }
                           }
                       }
                   }
               }

               renderer.Render(document);
               output.Content = htmlWriter.ToString().Trim();
               output.IsMarkdigMdSyntaxHighlightEnabled = _features.IsMarkdigMdSyntaxHighlightEnabled();
               output.Content = SanitizeText(output.Content);

               return output;
           }
       }
```

</details>

<details>

<summary>Features comparison</summary>

[Nuget Supported Markdown features](https://learn.microsoft.com/en-us/nuget/nuget-org/package-readme-on-nuget-org#supported-markdown-features)

|                       | Visual studio | Nuget Gallery |
| --------------------- | ------------- | ------------- |
| UseAdvancedExtensions | Yes           |               |
| UsePragmaLines        | Yes           |               |
| UseYamlFrontMatter    | Yes           |               |
| UseEmojiAndSmiley     | Yes           | Yes           |
| UseEmphasisExtras     | Yes           | Strikethrough |
| UseTaskLists          | Yes           | Yes           |
| UseGridTables         | advanced      | Yes           |
| UsePipeTables         | advanced      | Yes           |
| UseListExtras         | advanced      | Yes           |
| UseAutoLinks          | advanced      | Yes           |
| UseAlertBlocks        |               | Yes           |
| UseReferralLinks      |               | Yes           |
| UseAutoIdentifiers    | advanced      | Yes           |
| UseBootstrap          |               | Yes           |
| DisableHtml           |               | Yes           |

</details>

# GitLab

<details>

[GitLab markdown](https://docs.gitlab.com/user/markdown/)

```
GitLab Flavored Markdown consists of the following:

Core Markdown features, based on the CommonMark specification.
Extensions from GitHub Flavored Markdown.
Extensions made specifically for GitLab.
```

GitLab exclusive

[Multiline block quotes](https://docs.gitlab.com/user/markdown/#multiline-blockquote)

[Description lists](https://docs.gitlab.com/user/markdown/#description-lists) [html](https://developer.mozilla.org/en-US/docs/Web/HTML/Reference/Elements/dl)

[Front matter](https://docs.gitlab.com/user/markdown/#front-matter)

[Inline diff](https://docs.gitlab.com/user/markdown/#inline-diff)

[Table of contents](https://docs.gitlab.com/user/markdown/#table-of-contents)

[Includes](https://docs.gitlab.com/user/markdown/#includes)

[GitLab pipeline and parsing](https://gitlab.com/gitlab-org/gitlab/-/blob/master/doc/development/gitlab_flavored_markdown/banzai_pipeline_and_parsing.md)

```
Parsing and rendering GitLab Flavored Markdown into HTML involves different components:

Banzai pipeline and it's various filters
Markdown parser

The backend does all the processing for GLFM to HTML. This provides several benefits:

Security: We run robust sanitization which removes unknown tags, classes and ids.
References: Our reference syntax requires access to the database to resolve issues, etc, as well as redacting references in which the user has no access.
Consistency: We want to provide users with a consistent experience, which includes full support of the GLFM syntax and styling. Having a single place where the processing is done allows us to provide that.
Caching: We cache the HTML in our database when possible, such as for issue or MR descriptions, or comments.
Quick actions: We use a specialized pipeline to process quick actions, so that we can better detect them in Markdown text.

The frontend handles certain aspects when displaying:

Math blocks
Mermaid blocks
Enforcing certain limits, such as excessive number of math or mermaid blocks.


The Banzai pipeline
Named after the surf reef break in Hawaii, the Banzai pipeline consists of various filters (lib/banzai/filters) where Markdown and HTML is transformed in each one, in a pipeline fashion. Various pipelines (lib/banzai/pipeline) are defined, each with a different sequence of filters, such as AsciiDocPipeline, EmailPipeline.
The html-pipeline gem implements the pipeline/filter mechanism.
The primary pipeline is the FullPipeline, which is a combination of the PlainMarkdownPipeline and the GfmPipeline.

PlainMarkdownPipeline

This pipeline contains the filters for transforming raw Markdown into HTML, handled primarily by the Filter::MarkdownFilter.

Filter::MarkdownFilter

This filter interfaces with the actual Markdown parser. The parser uses our gitlab-glfm-markdown Ruby gem that uses the comrak Rust crate.
Text is passed into this filter, and by calling the specified parser engine, generates the corresponding basic HTML.

GfmPipeline

This pipeline contains all the filters that perform the additional transformations on raw HTML into what we consider rendered GLFM.
A Nokogiri document gets passed into each of these filters, and they perform the various transformations.
For example, EmojiFitler, CommitTrailersFilter, or SanitizationFilter.
Anything that can't be handled by the initial Markdown parsing gets handled by these filters.
Of specific note is the SanitizationFilter. This is critical for providing safe HTML from possibly malicious input.

PostProcessPipeline

The output from the FullPipeline gets cached in the database. However references have already been resolved. Based on
a users' permissions, they may not be able to see those references. PostProcessPipeline is responsible for redacting any
confidential information based on user permissions. These changes are never cached, as they need to get recomputed each time
they are displayed.
```

[Banzai pipelines](https://gitlab.com/gitlab-org/gitlab/-/tree/master/lib/banzai/pipeline)

[Banzai filters](https://gitlab.com/gitlab-org/gitlab/-/tree/master/lib/banzai/filter)

</details>

# GitHub

<details>

[Github flavoured markdown](https://github.github.com/gfm/) (does not mention emojis....)

Github also uses comrak, via https://github.com/gjtorikian/commonmarker. ( The first can require)[https://github.com/github/markup/blob/master/lib/github/markup/markdown.rb]

> It also includes extensions to the CommonMark spec as documented in the GitHub Flavored Markdown spec, such as support for tables, strikethroughs, and autolinking.

The GitHub pipeline is not open source - from the [readme](https://github.com/github/markup)

> only the first step is covered by this gem

> 2. The HTML is sanitized....
> 3. Syntax highlighting is performed on code blocks....
> 4. The HTML is passed through other filters that add special sauce, such as emoji, task lists, named anchors, CDN caching for images, and autolinking.

GitHub also has https://github.com/github/cmark-gfm available to use, [used by](https://github.com/github/jekyll-commonmark-ghpages). It is not the same as the rendering pipeline used by GitHub.com.

```
cmark-gfm is an extended version of the C reference implementation of CommonMark, a rationalized version of Markdown syntax with a spec. This repository adds GitHub Flavored Markdown extensions to the upstream implementation, as defined in the spec.
```

</details>

# Feature comparison - Nuget vs Repos

<details>

|                   | Nuget        | GitHub                    | GitLab                                                                 |
| ----------------- | ------------ | ------------------------- | ---------------------------------------------------------------------- |
| Tables            | Both types   | [Pipe][github-tables]     | [Pipe](https://docs.gitlab.com/user/markdown/#tables)                  |
| Emoji smiley      | Yes          | [link][github-emoji]      | [link](https://docs.gitlab.com/user/markdown/#emoji)                   |
| Autolinks         | Yes          | [link][github-als]        | [link](https://docs.gitlab.com/user/markdown/#url-auto-linking)        |
| Linkable headings | Yes          | [link][github-hls]        | [link](https://docs.gitlab.com/user/markdown/#heading-ids-and-links)   |
| Strikethrough     | Yes          | [link][github-strk]       | [link](https://docs.gitlab.com/user/markdown/#emphasis)                |
| Color chips       |              | [link][github-colorchips] | [link](https://docs.gitlab.com/user/markdown/#colors)                  |
| Alerts            | Yes          | [link][github-alerts]     | [link](https://docs.gitlab.com/user/markdown/#alerts)                  |
| Footnotes         |              | [link][github-footnotes]  | [link](https://docs.gitlab.com/user/markdown/#footnotes)               |
| Maths             |              | [link][github-maths]      | [link](https://docs.gitlab.com/user/markdown/#math-equations)          |
| Diagrams          |              | [link][github-diagrams]   | [link](https://docs.gitlab.com/user/markdown/#diagrams-and-flowcharts) |
| Embedded          |              | [link?][github-files]     | [audio][gitlab-audio] [video][gitlab-video]                            |
| Task lists        | Yes          | [link][github-tasklists]  | [link](https://docs.gitlab.com/user/markdown/#task-lists)              |
| Syntax highlight  | highlight.js | [link][github-sh]         | [link](https://docs.gitlab.com/user/markdown/#syntax-highlighting)     |
| Repo reference    |              | [link][github-reporefs]   | [refs][gitlab-reporefs] [placeholders][gitlab-phs]                     |

</details>

# Nuget syntax highlighting

<details>

NuGet.org uses [highlight.js](https://highlightjs.org/) for syntax highlighting of code blocks in README files.

You can see this in many places in https://github.com/NuGet/NuGetGallery, for example.

[View Helpers](https://github.com/NuGet/NuGetGallery/blob/53fe20adeb6195cb57ddc1d586ba5a3bac5cf3c1/src/NuGetGallery/App_Code/ViewHelpers.cshtml#L756)

[DisplayPackage.cshtml](https://github.com/NuGet/NuGetGallery/blob/53fe20adeb6195cb57ddc1d586ba5a3bac5cf3c1/src/NuGetGallery/Views/Packages/DisplayPackage.cshtml#L1637)

```cshtml
    @if (Model.IsMarkdigMdSyntaxHighlightEnabled)
    {
        @ViewHelpers.IncludeSyntaxHighlightScript();

        <script nonce="@Html.GetCSPNonce()">
        document.addEventListener('DOMContentLoaded', (event) => {
            document.querySelectorAll('pre code').forEach((el) => {
                hljs.highlightElement(el);
            });
        });
        </script>
    }
```

or
[syntaxhighlight.js](https://github.com/NuGet/NuGetGallery/blob/main/src/NuGetGallery/Scripts/gallery/syntaxhighlight.js#L1)

```js
    function syntaxHighlight() {
        document.querySelectorAll('pre code').forEach((el) => {
            hljs.highlightElement(el);
        });
    }
}
```

usage

[Manage.cshtml](https://github.com/NuGet/NuGetGallery/blob/53fe20adeb6195cb57ddc1d586ba5a3bac5cf3c1/src/NuGetGallery/Views/Packages/Manage.cshtml#L157)

```
    @if (Model.IsMarkdigMdSyntaxHighlightEnabled)
    {
        @ViewHelpers.IncludeSyntaxHighlightScript();
    }

    @ViewHelpers.SectionsScript(this)
    @Scripts.Render("~/Scripts/gallery/syntaxhighlight.min.js")

```

</details>

# Visual Studio Markdown Display

<details>

Is provided by

C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\CommonExtensions\Microsoft\Markdown\Microsoft.VisualStudio.Markdown.Platform.dll
C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\CommonExtensions\Microsoft\Markdown\Microsoft.VisualStudio.Markdown.Core.dll

A markdown preview is available in a PreviewMargin when a markdown file is open and the Preview button in the MarkdownToolBarMargin is toggled.
It uses `ITextBuffer.PostChanged` event to markdig Parse `ITextSnapshot.GetText()`;
A markdown preview is also shown from the Nuget Package Manager when a package with a readme is selected.

For Nuget the PackageReadmeControl uses the PreviewBuilder and IMarkdownPreview.

```chsarp
        public bool Initialize(IEditorOptionsFactoryService options)
        {
            if (options is null)
            {
                return false;
            }
            if (_markdownPreview is null)
            {
                // This class is marked as obsolete because the api hasn't been finalized, however we want to use IMarkdownPreview to maintain a centralized way of rendering markdown into html.
#pragma warning disable CS0618 // Type or member is obsolete
                var previewBuilder = new PreviewBuilder();
#pragma warning restore CS0618 // Type or member is obsolete
                previewBuilder.EditorOptions = options.GlobalOptions;
                _markdownPreview = previewBuilder.Build();
                descriptionMarkdownPreview.Content = _markdownPreview?.VisualElement;
            }
            return true;
        }

        private async Task UpdateMarkdownAsync(string markdown, CancellationToken token)
        {
            UpdateBusy(true);
            if (_markdownPreview is not null)
            {
                await TaskScheduler.Default;
                var success = await _markdownPreview.UpdateContentAsync(markdown, ScrollHint.None).PostOnFailureAsync(nameof(PackageReadmeControl));
                await NuGetUIThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(token);
                if (!success)
                {
                    ReadmeViewModel.ErrorWithReadme = true;
                    ReadmeViewModel.ReadmeMarkdown = string.Empty;
                }
            }
            UpdateBusy(false);
        }
```

The Markdig MarkdownPipeline shown at the top of this readdme is used and is rendered to HTML.
The WebView2 control displays a "template" html page that is passed the rendered HTML and also one of two css files based upon the darkness of the visual studio theme.
The template and other files are in C:\Program Files\Microsoft Visual Studio\2022\{version}\Common7\IDE\CommonExtensions\Microsoft\Markdown\Preview\Dependencies.
The template is daytona-md-template.html and the css files are highlight.css or daytona-highlight-dark.css.
**Visual studio currently does not use syntax highlighting.** It is possible, as follows:

I use [highlightjs](https://github.com/highlightjs/highlight.js) ( as used by Nuget ) by
a. Download and add highlight.min.js to the Dependencies folder ( run Notepad as administrator to save there )
b. Choose highlightjs theme css files for light and dark and insert them into highlight.css or daytona-highlight-dark.css.
https://github.com/highlightjs/highlight.js/blob/main/src/styles/github.css
https://github.com/highlightjs/highlight.js/blob/main/src/styles/github-dark.css
https://github.com/highlightjs/highlight.js/blob/main/src/styles/github-dark-dimmed.css

available on cdn - e.g https://cdn.jsdelivr.net/npm/highlight.js@11.9.0/styles/github.min.css
c. Add highlight.min.js with a script tag.
d. Apply highlightjs when the `<div id="___markdown-content___">` has its innerHTML set. A MutationObserver could have been used but I patched the property instead.

```html
<!DOCTYPE html>
<html>
  <head>
    <script
      src="http://scriptedhost.vs/plugin.js"
      type="text/javascript"
    ></script>
    <meta http-equiv="X-UA-Compatible" content="IE=Edge" />
    <meta charset="utf-8" />
    <link rel="stylesheet" href=" " id="CSSTheme" />
    <link rel="stylesheet" href=" " id="FontSize" />
    <script
      type="text/javascript"
      src="./DOMPurify/purify.min.js"
      id="DomPurifyTag"
    ></script>
    <script
      type="text/javascript"
      src="./daytonaHtmlContextMenu.js"
      id="HTMLContextMenuScript"
    ></script>
    <script type="text/javascript" src="./highlight.min.js"></script>
    <script>
      (() => {
        const desc = Object.getOwnPropertyDescriptor(
          Element.prototype,
          "innerHTML"
        );

        let isHighlighting = false;

        Object.defineProperty(Element.prototype, "innerHTML", {
          set(value) {
            desc.set.call(this, value);

            if (!isHighlighting && typeof hljs !== "undefined") {
              try {
                isHighlighting = true;
                document.querySelectorAll("pre code").forEach((el) => {
                  hljs.highlightElement(el);
                });
              } finally {
                isHighlighting = false;
              }
            }
          },
          get() {
            return desc.get.call(this);
          },
        });
      })();
    </script>
    <style>
      html,
      body {
      }
    </style>
    <title>Markdown Preview</title>
    <meta http-equiv="cache-control" content="no-cache" />
  </head>
  <body>
    <div id="___markdown-content___"></div>
  </body>
</html>
```

</details>

[github-emoji]: https://docs.github.com/en/get-started/writing-on-github/getting-started-with-writing-and-formatting-on-github/basic-writing-and-formatting-syntax#using-emojis
[github-colorchips]: https://docs.github.com/en/get-started/writing-on-github/getting-started-with-writing-and-formatting-on-github/basic-writing-and-formatting-syntax#supported-color-models
[github-maths]: https://docs.github.com/en/get-started/writing-on-github/working-with-advanced-formatting/writing-mathematical-expressions
[github-diagrams]: https://docs.github.com/en/get-started/writing-on-github/working-with-advanced-formatting/creating-diagrams
[github-footnotes]: https://docs.github.com/en/get-started/writing-on-github/getting-started-with-writing-and-formatting-on-github/basic-writing-and-formatting-syntax#footnotes
[github-alerts]: https://docs.github.com/en/get-started/writing-on-github/getting-started-with-writing-and-formatting-on-github/basic-writing-and-formatting-syntax#alerts
[github-tables]: https://github.github.com/gfm/#tables-extension-
[github-files]: https://docs.github.com/en/get-started/writing-on-github/working-with-advanced-formatting/attaching-files
[github-tasklists]: https://github.github.com/gfm/#task-list-items-extension-
[github-sh]: https://docs.github.com/en/get-started/writing-on-github/working-with-advanced-formatting/creating-and-highlighting-code-blocks#syntax-highlighting
[github-als]: https://github.github.com/gfm/#autolinks-extension-
[github-strk]: https://github.github.com/gfm/#strikethrough-extension-
[github-hls]: https://docs.github.com/en/get-started/writing-on-github/getting-started-with-writing-and-formatting-on-github/basic-writing-and-formatting-syntax#section-links
[github-reporefs]: https://docs.github.com/en/get-started/writing-on-github/working-with-advanced-formatting/autolinked-references-and-urls
[gitlab-audio]: https://docs.gitlab.com/user/markdown/#audio
[gitlab-video]: https://docs.gitlab.com/user/markdown/#videos
[gitlab-reporefs]: https://docs.gitlab.com/user/markdown/#gitlab-specific-references
[gitlab-phs]: https://docs.gitlab.com/user/markdown/#placeholders
