using System.IO;
using System.Text;

namespace NugetRepoReadme
{
    public static class DebugFileWriter
    {
        private static readonly StringBuilder s_contents = new StringBuilder();
        private static bool s_didWrite;

        public static string? FilePath { get; set; } = @"C:\Users\tonyh\Downloads\ref\debug_readme_rewriter.txt";

        public static void Write(string contents)
        {
            if (FilePath == null)
            {
                return;
            }

            s_didWrite = true;
            _ = s_contents.Append(contents);
        }

        public static void WriteToFile()
        {
            if (FilePath == null || !s_didWrite)
            {
                return;
            }

            File.WriteAllText(FilePath, s_contents.ToString());
        }
    }
}
