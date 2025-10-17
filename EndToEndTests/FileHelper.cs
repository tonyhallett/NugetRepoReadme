namespace EndToEndTests
{
    internal static class FileHelper
    {
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
