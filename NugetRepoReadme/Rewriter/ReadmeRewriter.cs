using System.Collections.Generic;
using System.Linq;
using NugetRepoReadme.AngleSharpDom;
using NugetRepoReadme.NugetValidation;
using NugetRepoReadme.Processing;
using NugetRepoReadme.ReadmeReplacement;
using NugetRepoReadme.RemoveReplace;
using NugetRepoReadme.RemoveReplace.Settings;
using NugetRepoReadme.Repo;

namespace NugetRepoReadme.Rewriter
{
    internal class ReadmeRewriter : IReadmeRewriter
    {
        internal const string ReadmeMarker = "{readme_marker}";
        private readonly IRewritableMarkdownElementsProvider _rewritableMarkdownElementsProvider;
        private readonly IReadmeReplacer _readmeReplacer;
        private readonly IReadmeMarkdownElementsProcessor _readmeMarkdownElementsProcessor;
        private readonly IRemoveReplacer _removeReplacer;
        private readonly IRepoUrlHelper _repoUrlHelper;

        internal ReadmeRewriter(
            IRewritableMarkdownElementsProvider rewritableMarkdownElementsProvider,
            IReadmeReplacer readmeReplacer,
            IReadmeMarkdownElementsProcessor readmeMarkdownElementsProcessor,
            IRemoveReplacer removeReplace,
            IRepoUrlHelper repoUrlHelper)
        {
            _rewritableMarkdownElementsProvider = rewritableMarkdownElementsProvider;
            _readmeReplacer = readmeReplacer;
            _readmeMarkdownElementsProcessor = readmeMarkdownElementsProcessor;
            _removeReplacer = removeReplace;
            _repoUrlHelper = repoUrlHelper;
        }

        public ReadmeRewriter()
            : this(
                new RewritableMarkdownElementsProvider(),
                new ReadmeReplacer(),
                new ReadmeMarkdownElementsProcessor(
                    new NuGetImageDomainValidator(NuGetTrustedImageDomains.Instance, new NuGetGitHubBadgeValidator()),
                    RepoUrlHelper.Instance,
                    new HtmlFragmentParser()),
                new RemoveReplacer(new RemoveReplaceRegexesFactory()),
                RepoUrlHelper.Instance)
        {
        }

        // the ref is the branch, tag or commit sha
        public ReadmeRewriterResult Rewrite(
            RewriteTagsOptions rewriteTagsOptions,
            string readme,
            string repoReadmeRelativePath,
            string? repoUrl,
            string @ref,
            RemoveReplaceSettings? removeReplaceSettings,
            IReadmeRelativeFileExists readmeRelativeFileExists)
        {
            RepoPaths? repoPaths = repoUrl != null ? RepoPaths.Create(repoUrl, @ref, repoReadmeRelativePath) : null;

            if (removeReplaceSettings != null)
            {
                ApplyRepoReadmeReplacementText(removeReplaceSettings.RemovalsOrReplacements, repoPaths, repoReadmeRelativePath);
                readme = _removeReplacer.RemoveReplace(readme, removeReplaceSettings);
            }

            return Rewrite(readme, repoPaths, rewriteTagsOptions, readmeRelativeFileExists);
        }

        private void ApplyRepoReadmeReplacementText(List<RemovalOrReplacement> removalsOrReplacements, RepoPaths? repoPaths, string readmeRelativePath)
        {
            if (repoPaths == null)
            {
                return;
            }

            removalsOrReplacements.Where(removalOrReplacement => removalOrReplacement.ReplacementText?.Contains(ReadmeMarker) == true).ToList().ForEach(replacement =>
            {
                string url = _repoUrlHelper.GetAbsoluteOrRepoAbsoluteUrl(readmeRelativePath, repoPaths, false);
                replacement.ReplacementText = replacement.ReplacementText!.Replace(ReadmeMarker, url);
            });
        }

        private IMarkdownElementsProcessResult Process(string readme, RepoPaths? repoPaths, RewriteTagsOptions rewriteTagsOptions, IReadmeRelativeFileExists readmeRelativeFileExists)
        {
            RelevantMarkdownElements relevantMarkdownElements = _rewritableMarkdownElementsProvider.GetRelevantMarkdownElementsWithSourceLocation(readme, rewriteTagsOptions == RewriteTagsOptions.None);
            return _readmeMarkdownElementsProcessor.Process(relevantMarkdownElements, repoPaths, rewriteTagsOptions, readmeRelativeFileExists);
        }

        private ReadmeRewriterResult Rewrite(string readme, RepoPaths? repoPaths, RewriteTagsOptions rewriteTagsOptions, IReadmeRelativeFileExists readmeRelativeFileExists)
        {
            bool unsupportedRepo = repoPaths == null;
            string? rewrittenReadme = null;
            RelevantMarkdownElements relevantMarkdownElements = _rewritableMarkdownElementsProvider.GetRelevantMarkdownElementsWithSourceLocation(readme, rewriteTagsOptions == RewriteTagsOptions.None);
            IMarkdownElementsProcessResult markdownElementsProcessResult = _readmeMarkdownElementsProcessor.Process(relevantMarkdownElements, repoPaths, rewriteTagsOptions, readmeRelativeFileExists);

            if (ShouldReplace())
            {
                IReplacementResult result = _readmeReplacer.Replace(readme, markdownElementsProcessResult.SourceReplacements);
                bool getResult = true;
                while (result.Result == null)
                {
                    foreach (IFurtherReplacement furtherReplacement in result.FurtherReplacements)
                    {
                        IMarkdownElementsProcessResult furthermarkdownElementsProcessResult = Process(furtherReplacement.ReplacementText, repoPaths, rewriteTagsOptions, readmeRelativeFileExists);
                        markdownElementsProcessResult.CombineIssues(furthermarkdownElementsProcessResult);
                        if (!ShouldReplace())
                        {
                            getResult = false;
                            break;
                        }

                        furtherReplacement.SetSourceReplacements(furthermarkdownElementsProcessResult.SourceReplacements);
                    }

                    result.ApplyFurtherReplacements();
                }

                if (getResult && result.Result != null)
                {
                    rewrittenReadme = result.Result;
                }
            }

            return new ReadmeRewriterResult(
                rewrittenReadme,
                markdownElementsProcessResult.UnsupportedImageDomains,
                markdownElementsProcessResult.MissingReadmeAssets,
                markdownElementsProcessResult.HasUnsupportedHtml,
                unsupportedRepo);

            bool ShouldReplace()
                => !markdownElementsProcessResult.HasUnsupportedHtml &&
                !markdownElementsProcessResult.HasErrors() &&
                !unsupportedRepo;
        }
    }
}
