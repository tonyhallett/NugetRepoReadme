using System.Collections.Generic;
using NugetRepoReadme.ReadmeReplacement;

namespace NugetRepoReadme.Processing
{
    internal interface IMarkdownElementsProcessResult
    {
        IEnumerable<SourceReplacement> SourceReplacements { get; }

        IEnumerable<string> UnsupportedImageDomains { get; }

        IEnumerable<string> MissingReadmeAssets { get; }

        bool HasUnsupportedHtml { get; }

        void CombineIssues(IMarkdownElementsProcessResult next);
    }
}
