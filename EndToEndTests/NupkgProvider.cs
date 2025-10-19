using System.Reflection;
using NuGet.Versioning;
using NugetBuildTargetsIntegrationTesting;
using NugetRepoReadme;

namespace EndToEndTests
{
    internal static class NupkgProvider
    {
        private const string NugetRepoReadme = "NugetRepoReadme";

        public static string GetNuPkgPath()
        {
            DirectoryInfo projectDirectory = GetProjectDirectory();
            string debugOrRelease = IsDebug.Value() ? "Debug" : "Release";
            DirectoryInfo debugOrReleaseDirectory = projectDirectory.GetDescendantDirectory("bin", debugOrRelease);
            FileInfo[] nupkgFiles = debugOrReleaseDirectory.GetFiles($"{NugetRepoReadme}*.nupkg", SearchOption.AllDirectories);
            return GetLatestVersion(nupkgFiles);
        }

        private static string GetLatestVersion(FileInfo[] nupkgFiles)
        {
            if (nupkgFiles.Length == 0)
            {
                throw new Exception("No nupkg files");
            }

            NuGetVersion? latestVersion = null;
            string? latestNuPkgPath = null;
            foreach (FileInfo nupkgFile in nupkgFiles)
            {
                string nuPkgPath = nupkgFile.FullName;
                (string PackageId, string Version) packageIdVersion = NuPkgHelper.GetPackageIdAndVersionFromNupkgPath(nuPkgPath);
                NuGetVersion version = new(packageIdVersion.Version);

                if (latestNuPkgPath == null || (latestVersion!.CompareTo(version) < 0))
                {
                    latestNuPkgPath = nuPkgPath;
                    latestVersion = version;
                }
            }

            return latestNuPkgPath!;
        }

        private static DirectoryInfo GetSolutionDirectory()
        {
            DirectoryInfo? directory = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
            while (directory != null && directory.Name != NugetRepoReadme)
            {
                directory = directory.Parent;
            }

            return directory ?? throw new Exception("Could not find solution directory");
        }

        private static DirectoryInfo GetProjectDirectory()
        {
            DirectoryInfo solutionDirectory = GetSolutionDirectory();
            return new DirectoryInfo(Path.Combine(solutionDirectory.FullName, NugetRepoReadme));
        }
    }
}
