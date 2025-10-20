namespace NugetRepoReadme.Repo
{
    internal interface IRepoReadmeFilePathsProvider
    {
        RepoReadmeFilePaths? GetRelativeReadmePath(string readmePath);
    }
}
