namespace NugetRepoReadme.Processing
{
    internal interface IRewritableMarkdownElementsProvider
    {
        RelevantMarkdownElements GetRelevantMarkdownElementsWithSourceLocation(
            string readme,
            bool excludeHtml);
    }
}
