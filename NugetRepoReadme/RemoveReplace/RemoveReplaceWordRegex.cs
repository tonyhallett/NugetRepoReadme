using System.Text.RegularExpressions;

namespace NugetRepoReadme.RemoveReplace
{
    internal class RemoveReplaceWordRegex
    {
        public RemoveReplaceWordRegex(string regexString, string? replacement)
        {
            Replacement = replacement ?? string.Empty;
            Regex = new Regex(regexString, RegexOptions.Compiled);
        }

        public string Replacement { get; internal set; }

        public Regex Regex { get; internal set; }
    }
}
