using System.Collections.Generic;
using System.Linq;

namespace NugetRepoReadme.ReadmeReplacement
{
    internal class FurtherReplacement : IReplacementParent, IFurtherReplacement
    {
        private readonly List<IReplacementParent> _children = new List<IReplacementParent>();

        public FurtherReplacement(SourceReplacement sourceReplacement)
        {
            Start = sourceReplacement.Start;
            End = sourceReplacement.End;
            ReplacementText = sourceReplacement.Replacement;
        }

        public int Start { get; }

        public int End { get; }

        public string ReplacementText { get; }

        public void AddChild(IReplacementParent child) => _children.Add(child);

        public string GetReplacement()
            => _children.Count == 0 ? ReplacementText : ReplacementStringBuilder.Build(ReplacementText, _children);

        public void SetSourceReplacements(IEnumerable<SourceReplacement> sourceReplacements)
            => SourceReplacements = sourceReplacements.OrderByDescending(r => r.Start).ToList();

        public IEnumerable<SourceReplacement> SourceReplacements { get; set; } = Enumerable.Empty<SourceReplacement>();
    }
}
