using System.Collections.Generic;

namespace NugetRepoReadme.MSBuildHelpers
{
    internal abstract class RequiredMetadataBase : IRequiredMetadata
    {
        private readonly List<string> _missingMetadataNames = new List<string>();

        [IgnoreMetadata]
        public IReadOnlyList<string> MissingMetadataNames => _missingMetadataNames;

        public void AddMissingMetadataName(string metadataName) => _missingMetadataNames.Add(metadataName);
    }
}
