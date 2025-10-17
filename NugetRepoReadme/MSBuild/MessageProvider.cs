﻿using System.Diagnostics.CodeAnalysis;
using NugetRepoReadme.Processing;
using NugetRepoReadme.RemoveReplace.Settings;

namespace NugetRepoReadme.MSBuild
{
    [ExcludeFromCodeCoverage]
    internal class MessageProvider : IMessageProvider
    {
        public static MessageProvider Instance { get; } = new MessageProvider();

        public string CouldNotParseRepositoryUrl(string? propertyValue)
            => $"Could not parse the repository url :{propertyValue}.  Use either {MsBuildPropertyItemNames.RepositoryUrlProperty} or {MsBuildPropertyItemNames.ReadmeRepositoryUrlProperty} to supply a GitHub or GitLab repository url.";

        public string CouldNotParseRewriteTagsOptionsUsingDefault(string propertyValue, RewriteTagsOptions defaultRewriteTagsOptions)
            => $"Could not parse the {MsBuildPropertyItemNames.RewriteTagsOptionsProperty}: {propertyValue}. Using the default: {defaultRewriteTagsOptions}";

        public string MissingReadmeAsset(string missingReadmeAsset) => $"Missing readme asset - {missingReadmeAsset}";

        public string ReadmeFileDoesNotExist(string readmeFilePath) => $"Readme file does not exist at '{readmeFilePath}'";

        public string ReadmeHasUnsupportedHTML() => "Readme has unsupported HTML";

        public string RemoveCommentsIdentifiersFormat()
            => $"MSBuild Property {nameof(ReadmeRewriterTask.RemoveCommentIdentifiers)} is either two semicolon delimited values: start and end, or start for removal from that point.";

        public string RemoveCommentsIdentifiersSameStartEnd()
            => $"MSBuild Property {nameof(ReadmeRewriterTask.RemoveCommentIdentifiers)} must have different start to end";

        public string RemoveReplaceWordsFileDoesNotExist(string filePath) => $"The remove/replace words file '{filePath}' does not exist.";

        public string RequiredMetadata(string metadataName, string itemSpec)
            => $"Metadata, {metadataName}, is required on item {MsBuildPropertyItemNames.ReadmeRemoveReplaceItem} '{itemSpec}'.";

        public string SameStartEndMetadata(string itemSpec)
            => $"{nameof(RemoveReplaceMetadata.Start)} and {nameof(RemoveReplaceMetadata.End)} metadata on item {MsBuildPropertyItemNames.ReadmeRemoveReplaceItem} '{itemSpec}' are the same value";

        public string UnsupportedCommentOrRegex(string itemSpec)
            => $"Unsupported {nameof(RemoveReplaceMetadata.CommentOrRegex)} metadata on item {MsBuildPropertyItemNames.ReadmeRemoveReplaceItem} '{itemSpec}'. Supported values {nameof(CommentOrRegex.Comment)} | {nameof(CommentOrRegex.Regex)}";

        public string UnsupportedImageDomain(string imageDomain) => $"Unsupported image domain found in README: {imageDomain}";
    }
}
