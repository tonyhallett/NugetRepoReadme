using System;

namespace NugetRepoReadme.Processing
{
    [Flags]
    internal enum RewriteTagsOptions
    {
        None = 0,
        RewriteImgTagsForSupportedDomains = 1,
        RewriteATags = 2,
        RewriteBrTags = 4,
        ExtractDetailsContentWithoutSummary = 8,
        RewriteAll = RewriteImgTagsForSupportedDomains | RewriteATags | RewriteBrTags | ExtractDetailsContentWithoutSummary,
        RemoveHtml = 16,
        ErrorOnHtml = 1 << 30,
    }
}
