using System.Collections.Generic;
using System.IO;

namespace NugetRepoReadme.Repo
{
    internal class RepoRelativeReadmePath : IRepoRelativeReadmePath
    {
        public string? GetRelativeReadmePath(string readmePath)
        {
            List<string> parentDirectories = new List<string>();
            FileInfo readmeFile = new FileInfo(readmePath);
            DirectoryInfo readmeDirectory = readmeFile.Directory!;
            DirectoryInfo searchDirectory = readmeDirectory;
            while (searchDirectory != null)
            {
                var gitDirectory = new DirectoryInfo(System.IO.Path.Combine(searchDirectory.FullName, ".git"));
                if (gitDirectory.Exists)
                {
                    break;
                }

                parentDirectories.Add(searchDirectory.Name);
                searchDirectory = searchDirectory.Parent;
            }

            if (searchDirectory == null)
            {
                return null;
            }

            if (parentDirectories.Count == 0)
            {
                return readmeFile.Name;
            }

            parentDirectories.Reverse();
            return string.Join(Path.DirectorySeparatorChar.ToString(), parentDirectories) + Path.DirectorySeparatorChar + readmeFile.Name;
        }
    }
}
