using NugetRepoReadme.RemoveReplace.Settings;

namespace NugetRepoReadme.RemoveReplace
{
    internal interface IRemoveReplaceRegexesFactory
    {
        IRemoveReplaceRegexes Create(RemoveReplaceSettings removeReplaceSettings);
    }
}
