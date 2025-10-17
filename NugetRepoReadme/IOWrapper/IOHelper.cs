using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace NugetRepoReadme.IOWrapper
{
    [ExcludeFromCodeCoverage]
    internal class IOHelper : IIOHelper
    {
        public static IOHelper Instance { get; } = new IOHelper();

        public string CombinePaths(string path1, string path2) => Path.Combine(path1, path2);

        public bool FileExists(string filePath) => File.Exists(filePath);

        public string ReadAllText(string readmePath) => File.ReadAllText(readmePath);

        public string[] ReadAllLines(string filePath) => File.ReadAllLines(filePath);
    }
}
