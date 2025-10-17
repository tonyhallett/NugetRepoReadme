using System;

namespace NugetRepoReadme.MSBuildHelpers
{
    [AttributeUsage(AttributeTargets.Property)]
    internal class RequiredMetadataAttribute : Attribute
    {
    }
}
