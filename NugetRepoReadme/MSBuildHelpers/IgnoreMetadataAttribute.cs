using System;

namespace NugetRepoReadme.MSBuildHelpers
{
    [AttributeUsage(AttributeTargets.Property)]
    internal class IgnoreMetadataAttribute : Attribute
    {
    }
}
