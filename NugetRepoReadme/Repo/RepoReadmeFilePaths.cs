namespace NugetRepoReadme.Repo
{
    public class RepoReadmeFilePaths
    {
        public RepoReadmeFilePaths(
            string repoDirectoryPath,
            string readmeDirectoryPath,
            string repoRelativeReadmeFilePath)
        {
            RepoDirectoryPath = repoDirectoryPath;
            ReadmeDirectoryPath = readmeDirectoryPath;
            RepoRelativeReadmeFilePath = repoRelativeReadmeFilePath;
        }

        public string RepoDirectoryPath { get; }

        public string ReadmeDirectoryPath { get; }

        public string RepoRelativeReadmeFilePath { get; }
    }
}
