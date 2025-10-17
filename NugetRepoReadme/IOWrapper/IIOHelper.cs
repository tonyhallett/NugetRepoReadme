namespace NugetRepoReadme.IOWrapper
{
    internal interface IIOHelper
    {
        bool FileExists(string filePath);

        string CombinePaths(string path1, string path2);

        string ReadAllText(string readmePath);

        string[] ReadAllLines(string filePath);
    }
}
