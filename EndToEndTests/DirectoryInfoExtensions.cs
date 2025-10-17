namespace EndToEndTests
{
    internal static class DirectoryInfoExtensions
    {
        public static DirectoryInfo GetDescendantDirectory(this DirectoryInfo directory, params string[] paths)
        {
            foreach (string path in paths)
            {
                directory = new DirectoryInfo(Path.Combine(directory.FullName, path));
            }

            return directory;
        }
    }
}
