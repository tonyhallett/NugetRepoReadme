using Microsoft.Build.Framework;

namespace NugetRepoReadme.RemoveReplace.Settings
{
    internal interface IRemoveReplaceSettingsProvider
    {
        IRemoveReplaceSettingsResult Provide(
            ITaskItem[]? removeReplaceItems,
            ITaskItem[]? removeReplaceWordsItems,
            string? removeCommentIdentifiers);
    }
}
