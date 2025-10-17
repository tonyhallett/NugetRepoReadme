using System;

namespace NugetRepoReadme.RemoveReplace.Settings
{
    internal class RemoveReplaceWord : IEquatable<RemoveReplaceWord>
    {
        public RemoveReplaceWord(string word, string? replacement)
        {
            Word = word;
            Replacement = replacement;
        }

        public string Word { get; }

        public string? Replacement { get; }

        public bool Equals(RemoveReplaceWord other) => other != null &&
               Word == other.Word &&
               Replacement == other.Replacement;
    }
}
