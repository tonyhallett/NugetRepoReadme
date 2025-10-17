using System.Text.RegularExpressions;
using NugetRepoReadme.RemoveReplace.Settings;

namespace NugetRepoReadme.RemoveReplace
{
    internal class RemoveCommentRegexes
    {
        public RemoveCommentRegexes(Regex startRegex, Regex? endRegex)
        {
            StartRegex = startRegex;
            EndRegex = endRegex;
        }

        public Regex StartRegex { get; }

        public Regex? EndRegex { get; }

        public static RemoveCommentRegexes Create(RemoveCommentIdentifiers removeCommentIdentifiers)
            => new RemoveCommentRegexes(
                CreateRegex(removeCommentIdentifiers.Start, false),
                removeCommentIdentifiers.End != null ? CreateRegex(removeCommentIdentifiers.End, false) : null);

        public static Regex CreateRegex(string commentIdentifier, bool exact)
        {
            string end = exact ? @"\s*" : @"\b[^>]*";
            string pattern = @"<!--\s*" + Regex.Escape(commentIdentifier) + end + "-->";
            return new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }
    }
}
