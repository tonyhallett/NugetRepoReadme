using NugetRepoReadme.Repo;

namespace NugetRepoReadme.Processing
{
    internal interface IReadmeMarkdownElementsProcessor
    {
        IMarkdownElementsProcessResult Process(
            RelevantMarkdownElements relevantMarkdownElements,
            RepoPaths? repoPaths,
            RewriteTagsOptions rewriteTagsOptions,
            IReadmeRelativeFileExists readmeRelativeFileExists);
    }
}
