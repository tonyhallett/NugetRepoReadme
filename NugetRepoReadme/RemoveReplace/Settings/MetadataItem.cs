using Microsoft.Build.Framework;
using NugetRepoReadme.MSBuild;

namespace NugetRepoReadme.RemoveReplace.Settings
{
    internal sealed class MetadataItem
    {
        public MetadataItem(RemoveReplaceMetadata removeReplaceMetadata, ITaskItem taskItem)
        {
            Metadata = removeReplaceMetadata;
            TaskItem = taskItem;
        }

        public RemoveReplaceMetadata Metadata { get; }

        public ITaskItem TaskItem { get; }
    }
}
