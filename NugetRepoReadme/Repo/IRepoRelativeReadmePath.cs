namespace NugetRepoReadme.Repo
{
    internal interface IRepoRelativeReadmePath
    {
        string? GetRelativeReadmePath(string readmePath);
    }
}
