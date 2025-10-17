using NugetRepoReadme.RemoveReplace.Settings;

namespace NugetRepoReadme.RemoveReplace
{
    internal interface IRemoveReplacer
    {
        string RemoveReplace(string text, RemoveReplaceSettings removeReplaceSettings);
    }
}
