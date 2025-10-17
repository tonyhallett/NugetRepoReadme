using System.Text.RegularExpressions;

namespace NugetRepoReadme.RemoveReplace
{
    internal interface IRemoveReplaceRegexes
    {
        bool Any { get; }

        MatchStartResult MatchStart(string line);

        Match MatchEnd(string afterStart);

        string? ReplaceWords(string line);
    }
}
