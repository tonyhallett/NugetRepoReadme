using System.Text.RegularExpressions;
using NugetRepoReadme.RemoveReplace.Settings;

namespace NugetRepoReadme.RemoveReplace
{
    internal class RegexRemovalOrReplacement
    {
        public RegexRemovalOrReplacement(Regex startRegex, Regex? endRegex, string? replacementText)
        {
            StartRegex = startRegex;
            EndRegex = endRegex;
            ReplacementText = replacementText;
        }

        public Regex StartRegex { get; }

        public Regex? EndRegex { get; }

        public string? ReplacementText { get; }

        private static Regex CreateRegex(CommentOrRegex commentOrRegex, string pattern)
            => commentOrRegex == CommentOrRegex.Regex
                ? new Regex(pattern, RegexOptions.Compiled) // should add IgnoreCase ?
                : RemoveCommentRegexes.CreateRegex(pattern, true);

        public static RegexRemovalOrReplacement Create(RemovalOrReplacement removalOrReplacement)
        {
            Regex startRegex = CreateRegex(removalOrReplacement.CommentOrRegex, removalOrReplacement.Start);
            Regex? endRegex = null;
            if (removalOrReplacement.End != null)
            {
                endRegex = CreateRegex(removalOrReplacement.CommentOrRegex, removalOrReplacement.End);
            }

            return new RegexRemovalOrReplacement(startRegex, endRegex, removalOrReplacement.ReplacementText);
        }
    }
}
