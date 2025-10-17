using System.Collections.Generic;

namespace NugetRepoReadme.ReadmeReplacement
{
    internal interface IFurtherReplacement
    {
        string ReplacementText { get; }

        void SetSourceReplacements(IEnumerable<SourceReplacement> sourceReplacements);
    }
}
