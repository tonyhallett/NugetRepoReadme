using Microsoft.Build.Framework;
using Moq;
using MSBuildTaskTestHelpers;
using NugetRepoReadme;
using NugetRepoReadme.IOWrapper;
using NugetRepoReadme.MSBuild;
using NugetRepoReadme.Processing;
using NugetRepoReadme.RemoveReplace.Settings;
using NugetRepoReadme.Rewriter;
using Tests.Utils;

namespace Tests
{
    internal sealed class ReadmeRewriterTask_Tests
    {
        private const string RepositoryUrl = "repositoryurl";
        private const string ProjectDirectoryPath = "projectdir";
        private const string RemoveCommentIdentifiers = "removeCommentIdentifiers";
        private readonly ITaskItem[] _removeReplaceTaskItems = [new Mock<ITaskItem>().Object];
        private DummyIOHelper _ioHelper = new();
        private Mock<IRemoveReplaceSettingsProvider> _mockRemoveReplaceSettingsProvider = new();
        private Mock<IReadmeRewriter> _mockReadmeRewriter = new();
        private DummyLogBuildEngine _dummyLogBuildEngine = new();
        private ReadmeRewriterTask _readmeRewriterTask = new();
        private TestRemoveReplaceSettingsResult _removeReplaceSettingsResult = new();

        private sealed class TestRemoveReplaceSettingsResult : IRemoveReplaceSettingsResult
        {
            public IReadOnlyList<string> Errors { get; set; } = [];

            public RemoveReplaceSettings? Settings { get; set; }
        }

        private sealed class DummyIOHelper : IIOHelper
        {
            public const string ReadmeText = "readme";

            public bool DoesFileExist { get; set; }

            public string CombinePaths(string path1, string path2) => $"{path1};{path2}";

            public string? FileExistsPath { get; private set; }

            public bool FileExists(string filePath)
            {
                FileExistsPath = filePath;
                return DoesFileExist;
            }

            public string ReadAllText(string readmePath) => ReadmeText;

            public string[] ReadAllLines(string filePath) => throw new NotImplementedException();
        }

        [SetUp]
        public void Setup()
        {
            _ioHelper = new DummyIOHelper();
            _mockRemoveReplaceSettingsProvider = new Mock<IRemoveReplaceSettingsProvider>();
            _mockReadmeRewriter = new Mock<IReadmeRewriter>();
            _dummyLogBuildEngine = new DummyLogBuildEngine();
            _readmeRewriterTask = new ReadmeRewriterTask
            {
                BuildEngine = _dummyLogBuildEngine,
                IOHelper = _ioHelper,
                ReadmeRewriter = _mockReadmeRewriter.Object,
                RemoveReplaceSettingsProvider = _mockRemoveReplaceSettingsProvider.Object,
                MessageProvider = new ConcatenatingArgumentsMessageProvider(),
                RepositoryUrl = RepositoryUrl,
                ProjectDirectoryPath = ProjectDirectoryPath,
            };
            _removeReplaceSettingsResult = new TestRemoveReplaceSettingsResult();
            _ = _mockRemoveReplaceSettingsProvider.Setup(removeReplaceSettingsProvider => removeReplaceSettingsProvider.Provide(_removeReplaceTaskItems, null, RemoveCommentIdentifiers))
                .Returns(_removeReplaceSettingsResult);
            _readmeRewriterTask.RemoveReplaceItems = _removeReplaceTaskItems;
            _readmeRewriterTask.RemoveCommentIdentifiers = RemoveCommentIdentifiers;
        }

        [Test]
        public void Should_Look_For_The_Readme_Relative_To_The_ProjectDirectoryPath()
        {
            _readmeRewriterTask.ReadmeRelativePath = "relativeReadme.md";

            _ = _readmeRewriterTask.Execute();

            Assert.That(_ioHelper.FileExistsPath, Is.EqualTo("projectdir;relativeReadme.md"));
        }

        [Test]
        public void Should_Default_The_ReadmeRelative_Path_To_Readme_In_Root()
        {
            _ = _readmeRewriterTask.Execute();

            Assert.That(_ioHelper.FileExistsPath, Is.EqualTo("projectdir;readme.md"));
        }

        [TestCase(null, "readme.md")]
        [TestCase("relativeReadme.md", "relativeReadme.md")]
        public void Should_Pass_The_ReadmeRelativeFileExists_To_The_ReadmeRewriter(
            string? readmeRelativePath,
            string expectedReadmeRelativePath)
        {
            SetupReadMeRewriter(
                new ReadmeRewriterResult(null, [], [], false, false),
                RewriteTagsOptions.None,
                readmeRelativePath,
                expectedReadmeRelativePath);

            _ = ExecuteReadmeExists();

            _mockReadmeRewriter.Verify(readmeRewriter => readmeRewriter.Rewrite(
                It.IsAny<RewriteTagsOptions>(),
                DummyIOHelper.ReadmeText,
                expectedReadmeRelativePath,
                RepositoryUrl,
                It.IsAny<string>(),
                _removeReplaceSettingsResult.Settings,
                It.Is<ReadmeRelativeFileExists>(readmeRelativeFileExists => readmeRelativeFileExists.ReadmeRelativePath == expectedReadmeRelativePath && readmeRelativeFileExists.ProjectDirectoryPath == ProjectDirectoryPath)));
        }

        [Test]
        public void Should_Log_Error_If_Readme_Does_Not_Exist()
        {
            bool result = _readmeRewriterTask.Execute();

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(false));
                Assert.That(
                    _dummyLogBuildEngine.SingleErrorMessage(),
                    Is.EqualTo("projectdir;readme.md"));
            });
        }

        private bool ExecuteReadmeExists()
        {
            _ioHelper.DoesFileExist = true;
            return _readmeRewriterTask.Execute();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_ReadmeRewriter_Rewrite_The_Readme_File_When_Exists(bool withSettings)
        {
            if (withSettings)
            {
                _removeReplaceSettingsResult.Settings = new RemoveReplaceSettings(null, [], []);
            }

            SetupReadMeRewriter(new ReadmeRewriterResult(null, [], [], false, false));
            _ = ExecuteReadmeExists();

            _mockReadmeRewriter.VerifyAll();
        }

        private void SetupReadMeRewriter(
            ReadmeRewriterResult readmeRewriterResult,
            RewriteTagsOptions rewriteTagsOptions = RewriteTagsOptions.None,
            string? readmeRelativePath = null,
            string? expectedReadmeRelativePath = null,
            string? expectedRepositoryUrl = null)
        {
            _readmeRewriterTask.ReadmeRelativePath = readmeRelativePath;
            _ = _mockReadmeRewriter.Setup(readmeRewriter => readmeRewriter.Rewrite(
                rewriteTagsOptions,
                DummyIOHelper.ReadmeText,
                expectedReadmeRelativePath ?? "readme.md",
                expectedRepositoryUrl ?? RepositoryUrl,
                It.IsAny<string>(),
                _removeReplaceSettingsResult.Settings,
                It.IsAny<IReadmeRelativeFileExists>()))
            .Returns(readmeRewriterResult);
        }

        [Test]
        public void Should_Log_Error_When_RepositoryUrl_Cannot_Be_Parsed()
        {
            SetupReadMeRewriter(new ReadmeRewriterResult(null, [], [], false, true));
            bool result = ExecuteReadmeExists();

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(false));
                Assert.That(
                    _dummyLogBuildEngine.SingleErrorMessage(),
                    Is.EqualTo(RepositoryUrl));
            });
        }

        [Test]
        public void Should_Log_Error_For_Every_Unsupported_Image_Domain()
        {
            ReadmeRewriterResult readmeRewriterResult = new(null, ["unsupported1", "unsupported2"], [], false, false);
            _ = _mockReadmeRewriter.Setup(readmeRewriter => readmeRewriter.Rewrite(
                It.IsAny<RewriteTagsOptions>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                RepositoryUrl,
                It.IsAny<string>(),
                It.IsAny<RemoveReplaceSettings>(),
                It.IsAny<IReadmeRelativeFileExists>())).Returns(readmeRewriterResult);

            bool result = ExecuteReadmeExists();

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(false));
                foreach ((string unsupportedImageDomain, string? error) in readmeRewriterResult.UnsupportedImageDomains.Zip(_dummyLogBuildEngine.ErrorMessages()))
                {
                    Assert.That(error, Is.EqualTo(unsupportedImageDomain));
                }
            });
        }

        [Test]
        public void Should_Log_Error_For_Every_Missing_Readme_Asset()
        {
            string[] missingReadmeAssets = ["/missing1", "/missing2"];
            SetupReadMeRewriter(new ReadmeRewriterResult(null, [], missingReadmeAssets, false, false));

            bool result = ExecuteReadmeExists();

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(false));
                foreach ((string missingReadme, string? error) in missingReadmeAssets.Zip(_dummyLogBuildEngine.ErrorMessages()))
                {
                    Assert.That(error, Is.EqualTo(missingReadme));
                }
            });
        }

        [Test]
        public void Should_Log_Error_For_Unsupported_HTML()
        {
            ReadmeRewriterResult readmeRewriterResult = new(null, [], [], true, false);
            _ = _mockReadmeRewriter.Setup(readmeRewriter => readmeRewriter.Rewrite(
                It.IsAny<RewriteTagsOptions>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                RepositoryUrl,
                It.IsAny<string>(),
                It.IsAny<RemoveReplaceSettings>(),
                It.IsAny<IReadmeRelativeFileExists>())).Returns(readmeRewriterResult);

            bool result = ExecuteReadmeExists();

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(false));
                Assert.That(_dummyLogBuildEngine.SingleErrorMessage, Is.EqualTo(nameof(IMessageProvider.ReadmeHasUnsupportedHTML)));
            });
        }

        [Test]
        public void Should_Write_Rewritten_Readme_To_OutputReadme()
        {
            _removeReplaceSettingsResult.Settings = new RemoveReplaceSettings(null, [], []);
            ReadmeRewriterResult readmeRewriterResult = new("rewrittenReadme", [], [], false, false);
            _ = _mockReadmeRewriter.Setup(readmeRewriter => readmeRewriter.Rewrite(
                It.IsAny<RewriteTagsOptions>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                RepositoryUrl,
                It.IsAny<string>(),
                _removeReplaceSettingsResult.Settings,
                It.IsAny<IReadmeRelativeFileExists>())).Returns(readmeRewriterResult);

            bool result = ExecuteReadmeExists();

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(true));
                Assert.That(readmeRewriterResult.RewrittenReadme, Is.EqualTo(_readmeRewriterTask.OutputReadme));
            });
        }

        /*
            Not advertising the use of both RewriteTagsOptions.  Needs more consideration.
        */

        [TestCase(RewriteTagsOptions.RewriteBrTags)]
        [TestCase(RewriteTagsOptions.RewriteAll)]
        [TestCase(RewriteTagsOptions.None)]
        public void Should_Parse_RewriteTagsOptions_Property_When_Provided(RewriteTagsOptions rewriteTagsOptions)
        {
            _readmeRewriterTask.RewriteTagsOptions = rewriteTagsOptions.ToString();

            RewriteTagsOptions_Test(rewriteTagsOptions);
        }

        [Test]
        public void Should_Log_Warning_When_RewriteTagsOptions_Property_Is_Malformed_And_Use_None()
        {
            _readmeRewriterTask.RewriteTagsOptions = "malformed";
            RewriteTagsOptions_Test(RewriteTagsOptions.None);
            Assert.That(
                    _dummyLogBuildEngine.SingleWarningMessage(),
                    Is.EqualTo("malformedNone"));
        }

        [Test]
        public void Should_Have_RewriteTagsOptions_None_When_Enum_Not_Set_And_No_Bool_Properties() => RewriteTagsOptions_Test(RewriteTagsOptions.None);

        [TestCase("true", RewriteTagsOptions.ErrorOnHtml)]
        [TestCase("True", RewriteTagsOptions.ErrorOnHtml)]
        [TestCase("false", RewriteTagsOptions.None)]
        [TestCase("malformed", RewriteTagsOptions.None)]
        [TestCase(null, RewriteTagsOptions.None)]
        public void Should_Have_RewriteTagsOptions_ErrorOnHtml_When_No_Enum_And_ErrorOnHtml(string? errorOnHtml, RewriteTagsOptions expectedRewriteTagsOptions)
        {
            _readmeRewriterTask.ErrorOnHtml = errorOnHtml;
            RewriteTagsOptions_Test(expectedRewriteTagsOptions);
        }

        [Test]
        public void Should_Have_RewriteTagsOptions_RemoveHtml_When_No_Enum_And_RemoveHtml_And_Not_ErrorOnHtml()
        {
            _readmeRewriterTask.RemoveHtml = "true";

            RewriteTagsOptions_Test(RewriteTagsOptions.RemoveHtml);
        }

        [Test]
        public void Should_Ignore_RemoveHtml_When_ErrorOnHtml()
        {
            _readmeRewriterTask.RemoveHtml = "true";
            _readmeRewriterTask.ErrorOnHtml = "true";

            RewriteTagsOptions_Test(RewriteTagsOptions.ErrorOnHtml);
        }

        [Test]
        public void Should_Add_Flag_ExtractDetailsContentWithoutSummary_When_Property_Is_True()
        {
            _readmeRewriterTask.ErrorOnHtml = "true";
            _readmeRewriterTask.ExtractDetailsContentWithoutSummary = "true";

            RewriteTagsOptions_Test(RewriteTagsOptions.ErrorOnHtml | RewriteTagsOptions.ExtractDetailsContentWithoutSummary);
        }

        // Set properties of _readmeRewriterTask beforehand
        private void RewriteTagsOptions_Test(RewriteTagsOptions expectedRewriteTagsOptions)
        {
            SetupReadMeRewriter(new ReadmeRewriterResult(null, [], [], false, false), expectedRewriteTagsOptions);
            _ = ExecuteReadmeExists();
        }

        [Test]
        public void Should_Log_Errors_From_RemoveReplaceSettingsProvider()
        {
            _ = _mockRemoveReplaceSettingsProvider.Setup(removeReplaceSettingsProvider => removeReplaceSettingsProvider.Provide(It.IsAny<ITaskItem[]?>(), It.IsAny<ITaskItem[]?>(), It.IsAny<string?>()))
                .Returns(new TestRemoveReplaceSettingsResult
                {
                    Errors = ["error1"],
                });
            _ = ExecuteReadmeExists();

            Assert.That(_dummyLogBuildEngine.SingleErrorMessage(), Is.EqualTo("error1"));
        }

        [Test]
        public void Should_Use_ReadmeRepositoryUrl_If_Provided()
        {
            _readmeRewriterTask.ReadmeRepositoryUrl = "readmeRepositoryUrl";

            SetupReadMeRewriter(
                new ReadmeRewriterResult(null, [], [], false, false),
                RewriteTagsOptions.None,
                null,
                null,
                _readmeRewriterTask.ReadmeRepositoryUrl);
            _ = ExecuteReadmeExists();
        }

        [Test]
        public void Should_Prefer_RepositoryRef_For_Ref()
            => RefTest("repositoryRef", "repositoryCommit", "repositoryBranch", "repositoryRef");

        [Test]
        public void Should_Prefer_RepositoryCommit_For_Ref_If_RepositoryRef_Null()
            => RefTest(null, "repositoryCommit", "repositoryBranch", "repositoryCommit");

        [Test]
        public void Should_Prefer_RepositoryBranch_For_Ref_If_RepositoryRef_And_RepositoryCommit_Null()
            => RefTest(null, null, "repositoryBranch", "repositoryBranch");

        [Test]
        public void Should_Default_Ref_To_Master_If_RepositoryRef_RepositoryCommit_And_RepositoryBranch_Null()
            => RefTest(null, null, null, "master");

        private void RefTest(string? repositoryRef, string? repositoryCommit, string? repositoryBranch, string expectedRef)
        {
            _readmeRewriterTask.RepositoryRef = repositoryRef;
            _readmeRewriterTask.RepositoryCommit = repositoryCommit;
            _readmeRewriterTask.RepositoryBranch = repositoryBranch;

            _ = _mockReadmeRewriter.Setup(readmeRewriter => readmeRewriter.Rewrite(
                It.IsAny<RewriteTagsOptions>(),
                DummyIOHelper.ReadmeText,
                It.IsAny<string>(),
                It.IsAny<string>(),
                expectedRef,
                It.IsAny<RemoveReplaceSettings?>(),
                It.IsAny<IReadmeRelativeFileExists>()))
            .Returns(new ReadmeRewriterResult(null, [], [], false, false));

            _ = ExecuteReadmeExists();

        }
    }
}
