using System.Collections.Generic;

namespace NugetRepoReadme.ReadmeReplacement
{
    internal interface IReplacementResult
    {
        string? Result { get; }

        IEnumerable<IFurtherReplacement> FurtherReplacements { get; }

        void ApplyFurtherReplacements();
    }
}
