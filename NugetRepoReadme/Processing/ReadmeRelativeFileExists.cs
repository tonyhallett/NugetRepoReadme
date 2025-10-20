using System.IO;
using NugetRepoReadme.Repo;

namespace NugetRepoReadme.Processing
{
    internal class ReadmeRelativeFileExists : IReadmeRelativeFileExists
    {
        public string RepoDirectoryPath { get; }

        public string ReadmeDirectoryPath { get; }

        public ReadmeRelativeFileExists(string repoDirectoryPath, string readmeDirectoryPath)
        {
            RepoDirectoryPath = repoDirectoryPath;
            ReadmeDirectoryPath = readmeDirectoryPath;
        }

        public bool Exists(string relativePath) => File.Exists(GetPath(relativePath));

        private string NormalizeDirectorySeparators(string path) => path
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);

        private string GetPath(string relativePath) => RepoRelative.RelativePathIsRepoRelative(relativePath)
                ? Path.Combine(RepoDirectoryPath, relativePath.TrimStart(RepoRelative.Char))
                : Path.Combine(ReadmeDirectoryPath, NormalizeDirectorySeparators(relativePath));
    }
}
