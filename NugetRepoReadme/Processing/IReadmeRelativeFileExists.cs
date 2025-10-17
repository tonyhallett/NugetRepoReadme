namespace NugetRepoReadme.Processing
{
    internal interface IReadmeRelativeFileExists
    {
        bool Exists(string relativePath);
    }
}
