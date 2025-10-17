using System.Collections.Generic;
using System.Text;

namespace NugetRepoReadme.ReadmeReplacement
{
    internal static class ReplacementStringBuilder
    {
        public static string Build(string text, IEnumerable<IReplacement> replacements)
        {
            var sb = new StringBuilder(text);
            foreach (IReplacement replacement in replacements)
            {
                _ = sb.Remove(replacement.Start, replacement.End - replacement.Start + 1)
                    .Insert(replacement.Start, replacement.GetReplacement());
            }

            return sb.ToString();
        }
    }
}
