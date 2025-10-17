using System.Collections.Generic;

namespace NugetRepoReadme.ReadmeReplacement
{
    internal class ReadmeReplacer : IReadmeReplacer
    {
        public IReplacementResult Replace(string text, IEnumerable<SourceReplacement> replacements)
            => new ReplacementResult(text, replacements);
    }
}
