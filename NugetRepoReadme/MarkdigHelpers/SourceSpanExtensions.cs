using System.Linq;
using Markdig.Syntax;

namespace NugetRepoReadme.MarkdigHelpers
{
    internal static class SourceSpanExtensions
    {
        public static SourceSpan Combine(this SourceSpan first, params SourceSpan[] otherSpans)
            => new SourceSpan(first.Start, first.End + otherSpans.Sum(other => other.Length));

        public static SourceSpan ExpandRight(this SourceSpan span, int count)
            => new SourceSpan(span.Start, span.End + count);

        public static SourceSpan ToEndOf(this SourceSpan start, SourceSpan end) => new SourceSpan(start.Start, end.End);
    }
}
