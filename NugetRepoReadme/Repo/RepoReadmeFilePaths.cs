namespace NugetRepoReadme.Repo
{
    public class RepoReadmeFilePaths
    {
        public RepoReadmeFilePaths(string repoDirectoryPath, string readmeDirectoryPath, string repoRelativeReadmePath)
        {
            RepoDirectoryPath = repoDirectoryPath;
            ReadmeDirectoryPath = readmeDirectoryPath;
            RepoRelativeReadmePath = repoRelativeReadmePath;
        }

        public string RepoDirectoryPath { get; }

        public string ReadmeDirectoryPath { get; }

        public string RepoRelativeReadmePath { get; }
    }
}
