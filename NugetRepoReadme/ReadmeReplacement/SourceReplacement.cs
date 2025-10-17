using Markdig.Syntax;

namespace NugetRepoReadme.ReadmeReplacement
{
    internal class SourceReplacement
    {
        public SourceReplacement(SourceSpan sourceSpan, string replacement, bool furtherProcessingRequired = false)
        {
            Start = sourceSpan.Start;
            End = sourceSpan.End;
            Replacement = replacement;
            FurtherProcessingRequired = furtherProcessingRequired;
        }

        public int Start { get; }

        public int End { get; }

        public string Replacement { get; }

        public bool FurtherProcessingRequired { get; }
    }
}
