using NugetRepoReadme.Processing;

namespace Tests.ProcessingTests
{
    internal sealed class ReadmeFileExists_Tests
    {
        private DirectoryInfo? _tempProjectDirectory;

        [SetUp]
        public void Setup() => _tempProjectDirectory = Directory.CreateTempSubdirectory();

        private ReadmeRelativeFileExists Initialize(string readmeRelativePath) => new(
                _tempProjectDirectory!.FullName,
                readmeRelativePath);

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Work_Relative_To_Repo_Root(bool exists)
        {
            ReadmeRelativeFileExists readmeRelativeFileExists = Initialize("readmedir/readme.md");
            string filePath = Path.Combine(_tempProjectDirectory!.FullName, "file.txt");
            if (exists)
            {
                File.WriteAllText(filePath, "test");
            }

            Assert.That(readmeRelativeFileExists.Exists("/file.txt"), Is.EqualTo(exists));
        }

        [TestCase("./")]
        [TestCase("")]
        public void Should_Work_Relative_To_Readme(string prefix)
        {
            ReadmeRelativeFileExists readmeRelativeFileExists = Initialize("readmedir/readme.md");
            string readmeDirectoryPath = Path.Combine(_tempProjectDirectory!.FullName, "readmedir");
            _ = Directory.CreateDirectory(readmeDirectoryPath);
            string filePath = Path.Combine(readmeDirectoryPath, "file.txt");
            File.WriteAllText(filePath, "test");

            Assert.That(readmeRelativeFileExists.Exists($"{prefix}file.txt"), Is.True);
        }

        [Test]
        public void Should_Work_Relative_To_Readme_Parent()
        {
            ReadmeRelativeFileExists readmeRelativeFileExists = Initialize("parent/readmedir/readme.md");
            string parentDirectoryPath = Path.Combine(_tempProjectDirectory!.FullName, "parent");
            _ = Directory.CreateDirectory(parentDirectoryPath);
            string filePath = Path.Combine(parentDirectoryPath, "file.txt");
            File.WriteAllText(filePath, "test");

            Assert.That(readmeRelativeFileExists.Exists("../file.txt"), Is.True);
        }

        [TearDown]
        public void Teardown() => _tempProjectDirectory!.Delete(true);
    }
}
