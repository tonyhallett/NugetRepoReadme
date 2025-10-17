using System;
using System.Collections.Generic;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using NugetRepoReadme.AngleSharpDom;
using NugetRepoReadme.MarkdigHelpers;
using NugetRepoReadme.NugetValidation;
using NugetRepoReadme.Repo;

namespace NugetRepoReadme.Processing
{
    internal class ReadmeMarkdownElementsProcessor : IReadmeMarkdownElementsProcessor
    {
        private readonly INuGetImageDomainValidator _nugetImageDomainValidator;
        private readonly IRepoUrlHelper _repoUrlHelper;
        private readonly IHtmlFragmentParser _htmlFragmentParser;

        public ReadmeMarkdownElementsProcessor(
            INuGetImageDomainValidator nugetImageDomainValidator,
            IRepoUrlHelper repoUrlHelper,
            IHtmlFragmentParser htmlFragmentParser)
        {
            _nugetImageDomainValidator = nugetImageDomainValidator;
            _repoUrlHelper = repoUrlHelper;
            _htmlFragmentParser = htmlFragmentParser;
        }

        public IMarkdownElementsProcessResult Process(
            RelevantMarkdownElements relevantMarkdownElements,
            RepoPaths? repoPaths,
            RewriteTagsOptions rewriteTagsOptions,
            IReadmeRelativeFileExists readmeRelativeFileExists)
        {
            var markdownElementsProcessResult = new MarkdownElementsProcessResult();
            ProcessLinkInlines(relevantMarkdownElements.LinkInlines, markdownElementsProcessResult, repoPaths, readmeRelativeFileExists);
            bool hasUnprocessedHtml = ProcessHtmlBlocks(relevantMarkdownElements.HtmlBlocks, markdownElementsProcessResult, repoPaths, rewriteTagsOptions);
            if (repoPaths != null)
            {
                bool hasUnprocessedInlines = ProcessHtmlInlines(relevantMarkdownElements.HtmlInlines, markdownElementsProcessResult, repoPaths, rewriteTagsOptions);
                if (hasUnprocessedInlines)
                {
                    hasUnprocessedHtml = true;
                }
            }

            if (hasUnprocessedHtml && rewriteTagsOptions.HasFlag(RewriteTagsOptions.ErrorOnHtml))
            {
                markdownElementsProcessResult.HasUnsupportedHtml = true;
            }

            return markdownElementsProcessResult;
        }

        private bool ProcessHtmlInlines(
            IEnumerable<HtmlInline> htmlInlines,
            MarkdownElementsProcessResult markdownElementsProcessResult,
            RepoPaths repoPaths,
            RewriteTagsOptions rewriteTagsOptions)
        {
            bool hasUnprocessedHtml = false;
            bool removeHtml = rewriteTagsOptions.HasFlag(RewriteTagsOptions.RemoveHtml);
            var removals = new List<HtmlInline>();
            foreach (HtmlInline htmlInline in htmlInlines)
            {
                if (rewriteTagsOptions.HasFlag(RewriteTagsOptions.RewriteBrTags) && htmlInline.IsBrTag())
                {
                    markdownElementsProcessResult.AddSourceReplacement(htmlInline.Span, "\\");
                    continue;
                }
                else if (rewriteTagsOptions.HasFlag(RewriteTagsOptions.RewriteATags) && HtmlInlineATag.TryCreate(htmlInline) is HtmlInlineATag htmlInlineATag)
                {
                    var anchorElement = _htmlFragmentParser.Parse(htmlInlineATag.Text) as IHtmlAnchorElement;
                    string? href = anchorElement!.GetAttribute("href");
                    if (href != null && HrefIsValid(href))
                    {
                        href = _repoUrlHelper.GetAbsoluteOrRepoAbsoluteUrl(href, repoPaths, false);
                        markdownElementsProcessResult.AddSourceReplacement(htmlInlineATag.Span, $"[{anchorElement!.TextContent}]({href})");
                        continue;
                    }
                }

                if (removeHtml)
                {
                    removals.Add(htmlInline);
                }
                else
                {
                    hasUnprocessedHtml = true;
                }
            }

            List<HtmlInlineCombiner.HtmlInlineStartEnd>? combinedRemovals = HtmlInlineCombiner.Combine(removals);
            if (combinedRemovals == null)
            {
                return true;
            }

            foreach ((HtmlInline start, HtmlInline? end) in combinedRemovals)
            {
                SourceSpan removalSpan = end == null ? start.Span : start.Span.ToEndOf(end.Span);
                SourceSpan removalSpanWithLineBreaks = removalSpan.ExpandRight(end?.RemovalNewLineLength() ?? start.RemovalNewLineLength());
                markdownElementsProcessResult.AddSourceReplacement(removalSpanWithLineBreaks, string.Empty);
            }

            return hasUnprocessedHtml;

            static bool HrefIsValid(string href)
            {
                href = href.Trim();
                return !string.IsNullOrWhiteSpace(href)
                    && !href.StartsWith("#", StringComparison.OrdinalIgnoreCase)
                    && !href.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase)
                    && !href.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)
                    && !href.StartsWith("tel:", StringComparison.OrdinalIgnoreCase);
            }
        }

        private bool ProcessHtmlBlocks(
            IEnumerable<HtmlBlock> htmlBlocks,
            MarkdownElementsProcessResult markdownElementsProcessResult,
            RepoPaths? repoPaths,
            RewriteTagsOptions rewriteTagsOptions)
        {
            bool hasUnprocessedHtml = false;
            bool removeHtml = rewriteTagsOptions.HasFlag(RewriteTagsOptions.RemoveHtml);
            foreach (HtmlBlock htmlBlock in htmlBlocks)
            {
                INode parsed = _htmlFragmentParser.Parse(htmlBlock);
                if (parsed is IHtmlDetailsElement htmlDetailsElement && rewriteTagsOptions.HasFlag(RewriteTagsOptions.ExtractDetailsContentWithoutSummary))
                {
                    string? replacementText = ExtractDetailsContentMarkdownWithoutSummary.Extract(htmlDetailsElement);
                    if (replacementText != null)
                    {
                        markdownElementsProcessResult.AddSourceReplacement(htmlBlock.Span, replacementText, true);
                        continue;
                    }
                }
                else if (parsed is IHtmlImageElement htmlImageElement)
                {
                    if (rewriteTagsOptions.HasFlag(RewriteTagsOptions.RewriteImgTagsForSupportedDomains))
                    {
                        var imgSrcAlt = ImgSrcAlt.TryGet(htmlImageElement);
                        if (imgSrcAlt != null)
                        {
                            ProcessImage(imgSrcAlt, htmlBlock);
                            continue;
                        }
                    }
                }

                if (removeHtml)
                {
                    markdownElementsProcessResult.RemoveHtmlBlock(htmlBlock);
                }
                else
                {
                    hasUnprocessedHtml = true;
                }
            }

            void ProcessImage(ImgSrcAlt srcAlt, HtmlBlock htmlBlock)
            {
                string src = srcAlt.Src;
                if (_repoUrlHelper.GetAbsoluteUri(src) is Uri absoluteUri)
                {
                    if (!_nugetImageDomainValidator.IsTrustedImageDomain(absoluteUri.OriginalString))
                    {
                        markdownElementsProcessResult.AddUnsupportedImageDomain(absoluteUri.Host);
                        return;
                    }
                }
                else
                {
                    if (repoPaths == null)
                    {
                        return;
                    }

                    src = _repoUrlHelper.GetRepoAbsoluteUrl(src, repoPaths, true)!;
                }

                markdownElementsProcessResult.AddSourceReplacement(htmlBlock.Span, $"![{srcAlt.Alt}]({src})");
            }

            return hasUnprocessedHtml;
        }

        private void ProcessLinkInlines(
            IEnumerable<LinkInline> linkInlines,
            MarkdownElementsProcessResult markdownElementsProcessResult,
            RepoPaths? repoPaths,
            IReadmeRelativeFileExists readmeRelativeFileExists)
        {
            foreach (LinkInline linkInline in linkInlines)
            {
                if (IgnoreLinkInline(linkInline))
                {
                    continue;
                }

                Uri? absoluteUri = _repoUrlHelper.GetAbsoluteUri(linkInline.Url);
                if (linkInline.IsImage && absoluteUri != null && !_nugetImageDomainValidator.IsTrustedImageDomain(absoluteUri.OriginalString))
                {
                    markdownElementsProcessResult.AddUnsupportedImageDomain(absoluteUri.Host);
                    continue;
                }

                if (absoluteUri == null && linkInline.Url != null && !readmeRelativeFileExists.Exists(linkInline.Url))
                {
                    markdownElementsProcessResult.AddMissingReadmeAsset(linkInline.Url);
                    continue;
                }

                if (repoPaths != null)
                {
                    SourceSpan urlSpan = linkInline.Reference != null ? linkInline.Reference.UrlSpan : linkInline.UrlSpan;
                    ProcessInlineUrl(linkInline.Url, linkInline.IsImage, repoPaths, urlSpan, markdownElementsProcessResult.AddSourceReplacement);
                }
            }
        }

        private bool IgnoreLinkInline(LinkInline linkInline)
        {
            if (linkInline.IsAutoLink || linkInline.Url == null)
            {
                return true;
            }

            string url = linkInline.Url.Trim();

            // ignore empty and fragments
            return string.IsNullOrEmpty(url) || url.StartsWith("#", StringComparison.Ordinal);
        }

        private void ProcessInlineUrl(
            string? url,
            bool isImage,
            RepoPaths repoPaths,
            SourceSpan span,
            Action<SourceSpan, string, bool> addSourceReplacement)
        {
            string? repoAbsoluteUrl = _repoUrlHelper.GetRepoAbsoluteUrl(url, repoPaths, isImage);
            if (repoAbsoluteUrl == null)
            {
                return;
            }

            addSourceReplacement(span, repoAbsoluteUrl, false);
        }
    }
}
