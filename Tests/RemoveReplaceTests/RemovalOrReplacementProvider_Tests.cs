using Microsoft.Build.Utilities;
using Moq;
using MSBuildTaskTestHelpers;
using NugetRepoReadme.IOWrapper;
using NugetRepoReadme.MSBuild;
using NugetRepoReadme.RemoveReplace.Settings;
using Tests.Utils;

namespace Tests.RemoveReplaceTests
{
    internal sealed class RemovalOrReplacementProvider_Tests
    {
        private Mock<IIOHelper> _mockIOHelper = new();
        private Mock<IMessageProvider> _mockMessageProvider = new();
        private RemovalOrReplacementProvider? _removalOrReplacementProvider;

        [SetUp]
        public void SetUp()
        {
            _mockIOHelper = new Mock<IIOHelper>();
            _mockMessageProvider = new Mock<IMessageProvider>();
            _removalOrReplacementProvider = new RemovalOrReplacementProvider(
                _mockIOHelper.Object,
                _mockMessageProvider.Object);
        }

        [TestCase(CommentOrRegex.Comment)]
        [TestCase(CommentOrRegex.Regex)]
        public void Should_Parse_To_CommentOrRegex(CommentOrRegex commentOrRegex)
        {
            var removeReplaceMetadata = new RemoveReplaceMetadata
            {
                CommentOrRegex = commentOrRegex.ToString(),
                Start = "start",
                ReplacementText = "....",
            };
            var taskItem = new TaskItem();

            var addError = new CollectingAddError();
            RemovalOrReplacement? removalOrReplacement = _removalOrReplacementProvider!.Provide(new MetadataItem(removeReplaceMetadata, taskItem), addError);

            Assert.Multiple(() =>
            {
                Assert.That(removalOrReplacement!.CommentOrRegex, Is.EqualTo(commentOrRegex));
                Assert.That(addError.Errors, Is.Empty);
            });
        }

        [Test]
        public void Should_Have_Error_When_Unsupported_CommentOrRegex()
        {
            _ = _mockMessageProvider.Setup(messageProvider => messageProvider.UnsupportedCommentOrRegex(
                "itemspec")).Returns("unsupported");
            var removeReplaceMetadata = new RemoveReplaceMetadata
            {
                CommentOrRegex = "unsupported",
                Start = "start",
                ReplacementText = "...",
            };
            var taskItem = new TaskItem("itemspec");

            var addError = new CollectingAddError();
            RemovalOrReplacement? removalOrReplacement = _removalOrReplacementProvider!.Provide(new MetadataItem(removeReplaceMetadata, taskItem), addError);

            Assert.Multiple(() =>
            {
                Assert.That(removalOrReplacement, Is.Null);
                Assert.That(addError.Single(), Is.EqualTo("unsupported"));
            });
        }

        [Test]
        public void Should_Have_Error_When_Start_End_Same()
        {
            _ = _mockMessageProvider.Setup(messageProvider => messageProvider.SameStartEndMetadata("itemspec"))
                .Returns("samestartend");
            var removeReplaceMetadata = new RemoveReplaceMetadata
            {
                CommentOrRegex = nameof(CommentOrRegex.Comment),
                Start = "same",
                End = "same",
                ReplacementText = "...",
            };
            var taskItem = new TaskItem("itemspec");

            var addError = new CollectingAddError();
            RemovalOrReplacement? removalOrReplacement = _removalOrReplacementProvider!.Provide(new MetadataItem(removeReplaceMetadata, taskItem), addError);

            Assert.Multiple(() =>
            {
                Assert.That(removalOrReplacement, Is.Null);
                Assert.That(addError.Single(), Is.EqualTo("samestartend"));
            });
        }

        [Test]
        public void Should_Use_Start_From_Metadata()
        {
            var removeReplaceMetadata = new RemoveReplaceMetadata
            {
                CommentOrRegex = nameof(CommentOrRegex.Regex),
                Start = "startregex",
                ReplacementText = "...",
            };
            var taskItem = new TaskItem();

            var addError = new CollectingAddError();
            RemovalOrReplacement? removalOrReplacement = _removalOrReplacementProvider!.Provide(new MetadataItem(removeReplaceMetadata, taskItem), addError);

            Assert.Multiple(() =>
            {
                Assert.That(removalOrReplacement!.Start, Is.EqualTo("startregex"));
                Assert.That(addError.Errors, Is.Empty);
            });
        }

        [TestCase("")]
        [TestCase(null)]
        public void Should_Have_Null_End_When_Null_Or_Empty(string? end)
        {
            var removeReplaceMetadata = new RemoveReplaceMetadata
            {
                CommentOrRegex = nameof(CommentOrRegex.Regex),
                Start = "startregex",
                End = end,
                ReplacementText = "...",
            };
            var taskItem = new TaskItem();

            var addError = new CollectingAddError();
            RemovalOrReplacement? removalOrReplacement = _removalOrReplacementProvider!.Provide(new MetadataItem(removeReplaceMetadata, taskItem), addError);

            Assert.Multiple(() =>
            {
                Assert.That(removalOrReplacement!.End, Is.Null);
                Assert.That(addError.Errors, Is.Empty);
            });
        }

        [Test]
        public void Should_Use_End_From_Metadata_When_Specified()
        {
            var removeReplaceMetadata = new RemoveReplaceMetadata
            {
                CommentOrRegex = nameof(CommentOrRegex.Regex),
                Start = "startregex",
                End = "endregex",
                ReplacementText = "...",
            };
            var taskItem = new TaskItem();

            var addError = new CollectingAddError();
            RemovalOrReplacement? removalOrReplacement = _removalOrReplacementProvider!.Provide(new MetadataItem(removeReplaceMetadata, taskItem), addError);

            Assert.Multiple(() =>
            {
                Assert.That(removalOrReplacement!.End, Is.EqualTo("endregex"));
                Assert.That(addError.Errors, Is.Empty);
            });
        }

        [Test]
        public void Should_Use_ReplacementText_From_Metadata()
        {
            var removeReplaceMetadata = new RemoveReplaceMetadata
            {
                CommentOrRegex = nameof(CommentOrRegex.Comment),
                Start = "start",
                ReplacementText = "replacement",
            };
            var taskItem = new TaskItem();

            var addError = new CollectingAddError();
            RemovalOrReplacement? removalOrReplacement = _removalOrReplacementProvider!.Provide(new MetadataItem(removeReplaceMetadata, taskItem), addError);

            Assert.Multiple(() =>
            {
                Assert.That(removalOrReplacement!.ReplacementText, Is.EqualTo("replacement"));
                Assert.That(addError.Errors, Is.Empty);
            });
        }

        [Test]
        public void Should_Use_ReplacementText_From_FileSystem_When_No_Metadata()
        {
            var removeReplaceMetadata = new RemoveReplaceMetadata
            {
                CommentOrRegex = nameof(CommentOrRegex.Comment),
                Start = "start",
                ReplacementText = string.Empty,
            };
            var testTaskItem = new TestTaskItem(null, "itemspec", new ItemSpecModifiersMetadata
            {
                FullPath = "fullpath",
            });

            _ = _mockIOHelper.Setup(ioHelper => ioHelper.FileExists("fullpath")).Returns(true);
            _ = _mockIOHelper.Setup(ioHelper => ioHelper.ReadAllText("fullpath")).Returns("filereplacement");

            var addError = new CollectingAddError();
            RemovalOrReplacement removalOrReplacement = _removalOrReplacementProvider!.Provide(new MetadataItem(removeReplaceMetadata, testTaskItem), addError)!;
            Assert.Multiple(() =>
            {
                Assert.That(removalOrReplacement!.ReplacementText, Is.EqualTo("filereplacement"));
                Assert.That(addError.Errors, Is.Empty);
            });
        }
    }
}
