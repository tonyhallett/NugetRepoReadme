using System;
using System.Diagnostics;
using Microsoft.Build.Framework;
using NugetRepoReadme.IOWrapper;
using NugetRepoReadme.MSBuild;
using NugetRepoReadme.MSBuildHelpers;
using NugetRepoReadme.Processing;
using NugetRepoReadme.RemoveReplace.Settings;
using NugetRepoReadme.Repo;
using NugetRepoReadme.Rewriter;
using InputOutputHelper = NugetRepoReadme.IOWrapper.IOHelper;

namespace NugetRepoReadme
{
    public class ReadmeRewriterTask : Microsoft.Build.Utilities.Task
    {
        internal const RewriteTagsOptions DefaultRewriteTagsOptions = Processing.RewriteTagsOptions.None;

        [Required]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public string ProjectDirectoryPath { get; set; }

        [Output]
        public string OutputReadme { get; set; }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        // one of these required
        public string? RepositoryUrl { get; set; }

        public string? ReadmeRepositoryUrl { get; set; }

        public string? ReadmeRelativePath { get; set; }

        #region for ref - in this order

        public string? RepositoryRef { get; set; }

        public string? RepositoryCommit { get; set; }

        public string? RepositoryBranch { get; set; }

        #endregion
        public string? ErrorOnHtml { get; set; }

        public string? RemoveHtml { get; set; }

        public string? ExtractDetailsContentWithoutSummary { get; set; }

        public string? RewriteTagsOptions { get; set; }

        public string? RemoveCommentIdentifiers { get; set; }

        public ITaskItem[]? RemoveReplaceItems { get; set; }

        public ITaskItem[]? RemoveReplaceWordsItems { get; set; }

        internal IIOHelper IOHelper { get; set; } = InputOutputHelper.Instance;

        internal IMessageProvider MessageProvider { get; set; } = MSBuild.MessageProvider.Instance;

        internal IReadmeRewriter ReadmeRewriter { get; set; } = new ReadmeRewriter();

        internal IRepoReadmeFilePathsProvider RepoReadmeFilePathsProvider { get; set; } = new RepoReadmeFilePathsProvider();

        internal IRemoveReplaceSettingsProvider RemoveReplaceSettingsProvider { get; set; } = new RemoveReplaceSettingsProvider(
            new MSBuildMetadataProvider(),
            new RemoveCommentsIdentifiersParser(MSBuild.MessageProvider.Instance),
            new RemovalOrReplacementProvider(InputOutputHelper.Instance, MSBuild.MessageProvider.Instance),
            new RemoveReplaceWordsProvider(InputOutputHelper.Instance, MSBuild.MessageProvider.Instance),
            MSBuild.MessageProvider.Instance);

        public override bool Execute()
        {
            LaunchDebuggerIfRequired();
            string readmePath = IOHelper.CombinePaths(ProjectDirectoryPath, ReadmeRelativePath ?? "readme.md");
            if (!IOHelper.FileExists(readmePath))
            {
                Log.LogError(MessageProvider.ReadmeFileDoesNotExist(readmePath));
            }
            else
            {
                RepoReadmeFilePaths? repoReadmeFilePaths = RepoReadmeFilePathsProvider.Provide(readmePath);
                if (repoReadmeFilePaths == null)
                {
                    Log.LogError(MessageProvider.CannotFindGitRepository());
                }
                else
                {
                    TryRewrite(IOHelper.ReadAllText(readmePath), repoReadmeFilePaths);
                }
            }

            DebugFileWriter.WriteToFile();
            return !Log.HasLoggedErrors;
        }

        private void LaunchDebuggerIfRequired()
        {
            if (Environment.GetEnvironmentVariable("DebugReadmeRewriter") != "1" || Debugger.IsAttached)
            {
                return;
            }

            _ = Debugger.Launch();
        }

        private void TryRewrite(string readmeContents, RepoReadmeFilePaths repoReadmeFilePaths)
        {
            IRemoveReplaceSettingsResult removeReplaceSettingsResult = RemoveReplaceSettingsProvider.Provide(
                RemoveReplaceItems,
                RemoveReplaceWordsItems,
                RemoveCommentIdentifiers);

            if (removeReplaceSettingsResult.Errors.Count > 0)
            {
                foreach (string error in removeReplaceSettingsResult.Errors)
                {
                    Log.LogError(error);
                }
            }
            else
            {
                Rewrite(readmeContents, repoReadmeFilePaths, removeReplaceSettingsResult.Settings);
            }
        }

        private string? GetRepositoryUrl() => ReadmeRepositoryUrl ?? RepositoryUrl;

        private string GetRef() => RepositoryRef ?? RepositoryCommit ?? RepositoryBranch ?? "master";

        private void Rewrite(
            string readmeContents,
            RepoReadmeFilePaths repoReadmeFilePaths,
            RemoveReplaceSettings? removeReplaceSettings)
        {
            string? repositoryUrl = GetRepositoryUrl();
            var readmeRelativeFileExists = new ReadmeRelativeFileExists(
                repoReadmeFilePaths.RepoDirectoryPath,
                repoReadmeFilePaths.ReadmeDirectoryPath);
            ReadmeRewriterResult readmeRewriterResult = ReadmeRewriter.Rewrite(
                GetRewriteTagsOptions(),
                readmeContents,
                repoReadmeFilePaths.RepoRelativeReadmeFilePath,
                repositoryUrl,
                GetRef(),
                removeReplaceSettings,
                readmeRelativeFileExists);

            foreach (string unsupportedImageDomain in readmeRewriterResult.UnsupportedImageDomains)
            {
                Log.LogError(MessageProvider.UnsupportedImageDomain(unsupportedImageDomain));
            }

            foreach (string missingReadmeAsset in readmeRewriterResult.MissingReadmeAssets)
            {
                Log.LogError(MessageProvider.MissingReadmeAsset(missingReadmeAsset));
            }

            if (readmeRewriterResult.HasUnsupportedHTML)
            {
                Log.LogError(MessageProvider.ReadmeHasUnsupportedHTML());
            }

            if (readmeRewriterResult.UnsupportedRepo)
            {
                Log.LogError(MessageProvider.CouldNotParseRepositoryUrl(repositoryUrl));
            }

            if (Log.HasLoggedErrors)
            {
                return;
            }

            OutputReadme = readmeRewriterResult.RewrittenReadme!;
        }

        private RewriteTagsOptions GetRewriteTagsOptions()
        {
            RewriteTagsOptions options = DefaultRewriteTagsOptions;
            if (RewriteTagsOptions != null)
            {
                if (Enum.TryParse(RewriteTagsOptions, out RewriteTagsOptions parsedOptions))
                {
                    options = parsedOptions;
                }
                else
                {
                    Log.LogWarning(MessageProvider.CouldNotParseRewriteTagsOptionsUsingDefault(RewriteTagsOptions, DefaultRewriteTagsOptions));
                }
            }
            else
            {
                bool errorsOnHtml = false;
                if (bool.TryParse(ErrorOnHtml, out bool errorOnHtml) && errorOnHtml)
                {
                    options = Processing.RewriteTagsOptions.ErrorOnHtml;
                    errorsOnHtml = true;
                }

                if (!errorsOnHtml && bool.TryParse(RemoveHtml, out bool removeHtml) && removeHtml)
                {
                    options = Processing.RewriteTagsOptions.RemoveHtml;
                }

                if (bool.TryParse(ExtractDetailsContentWithoutSummary, out bool extractDetailsContentWithoutSummary) && extractDetailsContentWithoutSummary)
                {
                    options |= Processing.RewriteTagsOptions.ExtractDetailsContentWithoutSummary;
                }
            }

            return options;
        }
    }
}
