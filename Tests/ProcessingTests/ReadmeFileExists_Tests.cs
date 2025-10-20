using NugetRepoReadme.Processing;

namespace Tests.ProcessingTests
{
    internal sealed class ReadmeFileExists_Tests
    {
        private DirectoryInfo? _tempRepoDirectory;
        private string _repoDirectoryPath = string.Empty;
        private string _readmeDirectoryPath = string.Empty;
        private ReadmeRelativeFileExists _readmeRelativeFileExists = new(string.Empty, string.Empty);

        [SetUp]
        public void Setup()
        {
            _tempRepoDirectory = Directory.CreateTempSubdirectory();
            _repoDirectoryPath = _tempRepoDirectory!.FullName;
        }

        private void Create(params string[] repoRelativeReadmeDirectories)
        {
            var parts = new List<string>(repoRelativeReadmeDirectories);
            parts.Insert(0, _repoDirectoryPath);
            _readmeDirectoryPath = Path.Combine(parts.ToArray());
            _readmeRelativeFileExists = new ReadmeRelativeFileExists(_repoDirectoryPath, _readmeDirectoryPath);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Work_Relative_To_Repo_Root(bool exists)
        {
            Create("readmedir");
            string repoRelativeFilePath = Path.Combine(_repoDirectoryPath, "file.txt");
            string readmeRelativeFilePath = Path.Combine(_readmeDirectoryPath, "file.txt");

            if (exists)
            {
                File.WriteAllText(repoRelativeFilePath, "test");
            }
            else
            {
                // test that it does not look in the readme directory
                _ = Directory.CreateDirectory(_readmeDirectoryPath);
                File.WriteAllText(readmeRelativeFilePath, "test");
            }

            Assert.That(_readmeRelativeFileExists.Exists("/file.txt"), Is.EqualTo(exists));
        }

        [TestCase("./")]
        [TestCase("")]
        public void Should_Work_Relative_To_Readme(string prefix)
        {
            Create("readmedir");
            _ = Directory.CreateDirectory(_readmeDirectoryPath);

            string filePath = Path.Combine(_readmeDirectoryPath, "file.txt");
            File.WriteAllText(filePath, "test");

            Assert.That(_readmeRelativeFileExists.Exists($"{prefix}file.txt"), Is.True);
        }

        [Test]
        public void Should_Work_Relative_To_Readme_Parent()
        {
            Create("parent", "readmedir");

            string parentDirectoryPath = Path.Combine(_repoDirectoryPath, "parent");
            _ = Directory.CreateDirectory(parentDirectoryPath);
            string filePath = Path.Combine(parentDirectoryPath, "file.txt");
            File.WriteAllText(filePath, "test");

            Assert.That(_readmeRelativeFileExists.Exists("../file.txt"), Is.True);
        }

        [TearDown]
        public void Teardown() => _tempRepoDirectory!.Delete(true);
    }
}
