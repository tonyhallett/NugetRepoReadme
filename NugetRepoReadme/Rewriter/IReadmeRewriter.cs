using NugetRepoReadme.Processing;
using NugetRepoReadme.RemoveReplace.Settings;

namespace NugetRepoReadme.Rewriter
{
    internal interface IReadmeRewriter
    {
        ReadmeRewriterResult Rewrite(
            RewriteTagsOptions rewriteTagsOptions,
            string readme,
            string repoReadmeRelativeFilePath,
            string? repoUrl,
            string @ref,
            RemoveReplaceSettings? removeReplaceSettings,
            IReadmeRelativeFileExists readmeRelativeFileExists);
    }
}
