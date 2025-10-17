using System.Collections.Generic;

namespace NugetRepoReadme.ReadmeReplacement
{
    internal interface IReadmeReplacer
    {
        IReplacementResult Replace(string text, IEnumerable<SourceReplacement> replacements);

    }
}
