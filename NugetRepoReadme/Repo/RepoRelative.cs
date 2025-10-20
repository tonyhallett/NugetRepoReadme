namespace NugetRepoReadme.Repo
{
    internal static class RepoRelative
    {
        public const char Char = '/';

        public static bool RelativePathIsRepoRelative(string relativePath) => relativePath.StartsWith(Char.ToString());
    }
}
