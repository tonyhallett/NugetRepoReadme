using System.IO;
using Microsoft.Build.Framework;

namespace NugetRepoReadme
{
    public class ReadmeWriterTask : Microsoft.Build.Utilities.Task
    {
        [Required]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public string ReadmeContents { get; set; }

        [Required]
        public string OutputReadme { get; set; }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public override bool Execute()
        {
            WriteAllTextEnsureDirectory(OutputReadme, ReadmeContents);
            return true;
        }

        public static void WriteAllTextEnsureDirectory(string path, string contents)
        {
            string? directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                _ = Directory.CreateDirectory(directory!);
            }

            File.WriteAllText(path, contents);
        }
    }
}
