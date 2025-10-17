using System.Collections.Generic;

namespace NugetRepoReadme.RemoveReplace.Settings
{
    internal interface IRemoveReplaceSettingsResult
    {
        IReadOnlyList<string> Errors { get; }

        RemoveReplaceSettings? Settings { get; }
    }
}
