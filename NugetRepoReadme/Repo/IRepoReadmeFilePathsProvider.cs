namespace NugetRepoReadme.Repo
{
    internal interface IRepoReadmeFilePathsProvider
    {
        RepoReadmeFilePaths? Provide(string readmePath);
    }
}
