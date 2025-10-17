using Microsoft.Build.Framework;

namespace NugetRepoReadme.MSBuildHelpers
{
    internal interface IMSBuildMetadataProvider
    {
        T GetCustomMetadata<T>(ITaskItem item)
            where T : new();
    }
}
