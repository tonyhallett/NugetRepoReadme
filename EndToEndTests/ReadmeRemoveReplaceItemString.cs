using System.Text;
using NugetRepoReadme.MSBuild;
using NugetRepoReadme.RemoveReplace.Settings;

namespace EndToEndTests
{
    internal static class ReadmeRemoveReplaceItemString
    {
        public static string Create(string include, IEnumerable<string> metadata)
        {
            var sb = new StringBuilder();
            _ = sb.AppendLine(@$"<{MsBuildPropertyItemNames.ReadmeRemoveReplaceItem} Include=""{include}"">");
            foreach (string meta in metadata)
            {
                _ = sb.AppendLine(meta);
            }

            _ = sb.AppendLine($@"</{MsBuildPropertyItemNames.ReadmeRemoveReplaceItem}>");
            return sb.ToString();
        }

        public static string StartElement(string start) => CreateMetadataElement(nameof(RemoveReplaceMetadata.Start), start);

        public static string EndElement(string end) => CreateMetadataElement(nameof(RemoveReplaceMetadata.End), end);

        public static string CommentOrRegexElement(CommentOrRegex commentOrRegex) => CreateMetadataElement(nameof(RemoveReplaceMetadata.CommentOrRegex), commentOrRegex.ToString());

        public static string ReplacementTextElement(string replacementText) => CreateMetadataElement(nameof(RemoveReplaceMetadata.ReplacementText), replacementText);

        private static string CreateMetadataElement(string metadataName, string contents)
            => $"<{metadataName}>{contents}</{metadataName}>";
    }
}
