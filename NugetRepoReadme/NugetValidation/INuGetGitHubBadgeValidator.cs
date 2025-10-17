namespace NugetRepoReadme.NugetValidation
{
    internal interface INuGetGitHubBadgeValidator
    {
        bool Validate(string url);
    }
}
