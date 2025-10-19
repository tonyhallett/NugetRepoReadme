﻿using NugetRepoReadme.Processing;

namespace NugetRepoReadme.MSBuild
{
    internal interface IMessageProvider
    {
        string RequiredMetadata(string metadataName, string itemSpec);

        string UnsupportedCommentOrRegex(string itemSpec);

        string SameStartEndMetadata(string itemSpec);

        string RemoveCommentsIdentifiersFormat();

        string RemoveCommentsIdentifiersSameStartEnd();

        string UnsupportedImageDomain(string imageDomain);

        string ReadmeFileDoesNotExist(string readmeFilePath);

        string CouldNotParseRepositoryUrl(string? propertyValue);

        string CouldNotParseRewriteTagsOptionsUsingDefault(string propertyValue, RewriteTagsOptions defaultRewriteTagsOptions);

        string ReadmeHasUnsupportedHTML();

        string MissingReadmeAsset(string missingReadmeAsset);

        string RemoveReplaceWordsFileDoesNotExist(string filePath);

        string CannotFindGitRepository();
    }
}
