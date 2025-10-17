using System.Collections.Generic;
using System.Linq;

namespace NugetRepoReadme.ReadmeReplacement
{
    internal class ReplacementResult : IReplacementResult
    {
        private readonly string _text;

        private List<FurtherReplacement> _furtherReplacements = new List<FurtherReplacement>();

        public IEnumerable<IFurtherReplacement> FurtherReplacements => _furtherReplacements;

        private readonly List<IReplacementParent> _roots = new List<IReplacementParent>();

        public ReplacementResult(string text, IEnumerable<SourceReplacement> replacements)
        {
            _text = text;
            bool furtherProcessingRequired = false;
            IOrderedEnumerable<SourceReplacement> ordered = replacements.OrderByDescending(replacements => replacements.Start);
            foreach (SourceReplacement replacement in ordered)
            {
                var furtherReplacement = new FurtherReplacement(replacement);
                if (replacement.FurtherProcessingRequired)
                {
                    _furtherReplacements.Add(furtherReplacement);
                    furtherProcessingRequired = true;
                }

                _roots.Add(furtherReplacement);
            }

            if (furtherProcessingRequired)
            {
                return;
            }

            BuildResult();
        }

        private void BuildResult() => Result = ReplacementStringBuilder.Build(_text, _roots);

        public string? Result { get; internal set; }

        public void ApplyFurtherReplacements()
        {
            if (_furtherReplacements.Count == 0)
            {
                throw new System.Exception("already processed");
            }

            var newFurtherReplacements = new List<FurtherReplacement>();
            foreach (FurtherReplacement furtherReplacement in _furtherReplacements)
            {
                foreach (SourceReplacement sourceReplacement in furtherReplacement.SourceReplacements)
                {
                    var newFurtherReplacement = new FurtherReplacement(sourceReplacement);
                    furtherReplacement.AddChild(newFurtherReplacement);
                    if (sourceReplacement.FurtherProcessingRequired)
                    {
                        newFurtherReplacements.Add(newFurtherReplacement);
                    }
                }
            }

            _furtherReplacements = newFurtherReplacements;
            if (newFurtherReplacements.Count != 0)
            {
                return;
            }

            BuildResult();
        }
    }
}
