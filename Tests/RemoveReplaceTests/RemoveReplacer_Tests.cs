using Moq;
using NugetRepoReadme.RemoveReplace;
using NugetRepoReadme.RemoveReplace.Settings;

namespace Tests.RemoveReplaceTests
{
    internal sealed class RemoveReplacer_Tests
    {
        private RemoveReplacer _removeReplacer = new(new RemoveReplaceRegexesFactory());

        [SetUp]
        public void Setup() => _removeReplacer = new RemoveReplacer(new RemoveReplaceRegexesFactory());

        [Test]
        public void Should_Not_When_No_Regexes_From_RemoveReplaceSettings()
        {
            const string readMeContent = @"
readme not looked at
";
            var removeReplaceSettings = new RemoveReplaceSettings(null, [], []);
            string notReplaced = _removeReplacer.RemoveReplace(readMeContent, removeReplaceSettings);
            Assert.That(notReplaced, Is.EqualTo(readMeContent));
        }

        [Test]
        public void Should_Remove_Commented_Sections()
        {
            const string readMeContent = @"
This is visible
<!-- remove-start -->
This is removed
<!-- remove-end -->
This is also visible
";
            var removeCommentIdentifiers = new RemoveCommentIdentifiers("remove-start", "remove-end");
            var removeReplaceSettings = new RemoveReplaceSettings(removeCommentIdentifiers, [], []);
            string rewrittenReadMe = _removeReplacer.RemoveReplace(readMeContent, removeReplaceSettings)!;

            const string expectedReadMeContent = @"
This is visible
This is also visible
";

            Assert.That(rewrittenReadMe, Is.EqualTo(expectedReadMeContent));
        }

        [Test]
        public void Should_Remove_Multiple_Commented_Sections()
        {
            const string readMeContent = @"
This is visible
<!-- remove-start 1 -->
This is removed
<!-- remove-end 1 -->
This is also visible
<!-- remove-start 2 -->
This is removed
<!-- remove-end 2 -->
This too is visible
";
            var removeCommentIdentifiers = new RemoveCommentIdentifiers("remove-start", "remove-end");
            var removeReplaceSettings = new RemoveReplaceSettings(removeCommentIdentifiers, [], []);
            string rewrittenReadMe = _removeReplacer.RemoveReplace(readMeContent, removeReplaceSettings)!;

            const string expectedReadMeContent = @"
This is visible
This is also visible
This too is visible
";

            Assert.That(rewrittenReadMe, Is.EqualTo(expectedReadMeContent));
        }

        [Test]
        public void Should_Replace_With_RemovalOrReplacement_Comments()
        {
            const string readMeContent = @"
This is visible
<!-- remove-start 1 -->
This is removed
<!-- remove-end 1 -->
This is also visible
<!-- remove-start 1 x -->
This is removed
<!-- remove-end 1 x -->
This too is visible
";

            var removalOrReplacement1 = new RemovalOrReplacement(CommentOrRegex.Comment, "remove-start 1", "remove-end 1", "r1");
            var removalOrReplacement1X = new RemovalOrReplacement(CommentOrRegex.Comment, "remove-start 1 x", "remove-end 1 x", "rx");
            var removeReplaceSettings = new RemoveReplaceSettings(null, [removalOrReplacement1, removalOrReplacement1X], []);
            string rewrittenReadMe = _removeReplacer.RemoveReplace(readMeContent, removeReplaceSettings)!;

            const string expectedReadMeContent = @"
This is visible
r1
This is also visible
rx
This too is visible
";

            Assert.That(rewrittenReadMe, Is.EqualTo(expectedReadMeContent));
        }

        [Test]
        public void Should_Remove_With_Regex()
        {
            const string readMeContent = @"
This is visible
# Remove 1
This is removed
# Remove 2
This is also visible
";

            RemovalOrReplacement replacement = new(CommentOrRegex.Regex, "# Remove 1", "# Remove 2", null);
            var removeReplaceSettings = new RemoveReplaceSettings(null, [replacement], []);
            string rewrittenReadMe = _removeReplacer.RemoveReplace(readMeContent, removeReplaceSettings)!;

            const string expectedReadMeContent = @"
This is visible
This is also visible
";

            Assert.That(rewrittenReadMe, Is.EqualTo(expectedReadMeContent));
        }

        [Test]
        public void Should_Replace_With_Regex()
        {
            const string readMeContent = @"
This is visible
# Remove 1
This is removed
# Remove 2
This is also visible
";

            RemovalOrReplacement replacement = new(CommentOrRegex.Regex, "# Remove 1", "# Remove 2", "Replaced Text");
            var removeReplaceSettings = new RemoveReplaceSettings(null, [replacement], []);
            string rewrittenReadMe = _removeReplacer.RemoveReplace(readMeContent, removeReplaceSettings)!;

            const string expectedReadMeContent = @"
This is visible
Replaced Text
This is also visible
";

            Assert.That(rewrittenReadMe, Is.EqualTo(expectedReadMeContent));
        }

        [Test]
        public void Should_Replace_With_Regex_Multiple()
        {
            const string readMeContent = @"
This is visible
# Remove 1
This is removed
# Remove 2
This is also visible
# Remove A
This is also removed
# Remove B
and so is this
";

            RemovalOrReplacement replacement1 = new(CommentOrRegex.Regex, "# Remove 1", "# Remove 2", "Replaced Text");
            RemovalOrReplacement replacement2 = new(CommentOrRegex.Regex, "# Remove A", "# Remove B", "Replaced Text 2");
            var removeReplaceSettings = new RemoveReplaceSettings(null, [replacement1, replacement2], []);
            string rewrittenReadMe = _removeReplacer.RemoveReplace(readMeContent, removeReplaceSettings)!;

            const string expectedReadMeContent = @"
This is visible
Replaced Text
This is also visible
Replaced Text 2
and so is this
";

            Assert.That(rewrittenReadMe, Is.EqualTo(expectedReadMeContent));
        }

        [TestCase(null)]
        [TestCase("remove-end")]
        public void Should_Remove_Remaining_Commented(string? commentEnd)
        {
            const string readMeContent = @"
This is visible
<!-- remove-start -->
This is removed
";
            var removeCommentIdentifiers = new RemoveCommentIdentifiers("remove-start", commentEnd);
            var removeReplaceSettings = new RemoveReplaceSettings(removeCommentIdentifiers, [], []);
            string rewrittenReadMe = _removeReplacer.RemoveReplace(readMeContent, removeReplaceSettings)!;

            const string expectedReadMeContent = @"
This is visible
";

            Assert.That(rewrittenReadMe, Is.EqualTo(expectedReadMeContent));
        }

        [Test]
        public void Should_Remove_Remaining_Regex()
        {
            const string readMeContent = @"
This is visible
# Remove
This is removed
";
            RemovalOrReplacement regexRemoval = new(CommentOrRegex.Regex, "# Remove", null, null);
            var removeReplaceSettings = new RemoveReplaceSettings(null, [regexRemoval], []);
            string rewrittenReadMe = _removeReplacer.RemoveReplace(readMeContent, removeReplaceSettings)!;

            const string expectedReadMeContent = @"
This is visible
";

            Assert.That(rewrittenReadMe, Is.EqualTo(expectedReadMeContent));
        }

        [Test]
        public void Should_Replace_Remaining_Regex()
        {
            const string readMeContent = @"
This is visible
# Replace remaining
This is removed
";
            RemovalOrReplacement regexRemoval = new(CommentOrRegex.Regex, "# Replace remaining", null, "Replaced");
            var removeReplaceSettings = new RemoveReplaceSettings(null, [regexRemoval], []);
            string rewrittenReadMe = _removeReplacer.RemoveReplace(readMeContent, removeReplaceSettings)!;

            const string expectedReadMeContent = @"
This is visible
Replaced";

            Assert.That(rewrittenReadMe, Is.EqualTo(expectedReadMeContent));
        }

        [Test]
        public void Should_Replace_Remaining_Comment()
        {
            const string readMeContent = @"
This is visible
<!-- Replace remaining -->
This is removed
";
            RemovalOrReplacement commentRemoval = new(CommentOrRegex.Comment, "Replace remaining", null, "Replaced");
            var removeReplaceSettings = new RemoveReplaceSettings(null, [commentRemoval], []);
            string rewrittenReadMe = _removeReplacer.RemoveReplace(readMeContent, removeReplaceSettings)!;

            const string expectedReadMeContent = @"
This is visible
Replaced";

            Assert.That(rewrittenReadMe, Is.EqualTo(expectedReadMeContent));
        }

        [Test]
        public void Should_Be_Able_To_Replace_Words()
        {
            var removeReplaceSettings = new RemoveReplaceSettings(null, [], []);
            var mockRemoveReplaceRegexes = new Mock<IRemoveReplaceRegexes>();
            _ = mockRemoveReplaceRegexes.SetupGet(removeReplaceRegexes => removeReplaceRegexes.Any).Returns(true);
            _ = mockRemoveReplaceRegexes.Setup(removeReplaceRegexes => removeReplaceRegexes.ReplaceWords(It.Is<string>(line => line.Contains("Some markdown")))).Returns((string?)null);
            _ = mockRemoveReplaceRegexes.Setup(removeReplaceRegexes => removeReplaceRegexes.ReplaceWords(It.Is<string>(line => !line.Contains("Some markdown")))).Returns(string.Empty);
            _ = mockRemoveReplaceRegexes.Setup(removeReplaceRegexes => removeReplaceRegexes.MatchStart(It.IsAny<string>())).Returns(new MatchStartResult(System.Text.RegularExpressions.Match.Empty));
            _ = mockRemoveReplaceRegexes.Setup(removeReplaceRegexes => removeReplaceRegexes.MatchEnd(It.IsAny<string>())).Returns(System.Text.RegularExpressions.Match.Empty);
            var mockRemoveReplaceRegexesFactory = new Mock<IRemoveReplaceRegexesFactory>();
            _ = mockRemoveReplaceRegexesFactory.Setup(removeReplaceRegexesFactory => removeReplaceRegexesFactory.Create(removeReplaceSettings))
                .Returns(mockRemoveReplaceRegexes.Object);
            var removeReplacer = new RemoveReplacer(mockRemoveReplaceRegexesFactory.Object);

            const string input =
@"<detail>
<summary>Summary</summary
Some markdown
</detail>";
            const string expected = @"Some markdown
";
            string removeReplaced = removeReplacer.RemoveReplace(input, removeReplaceSettings);

            Assert.That(removeReplaced, Is.EqualTo(expected));
        }

        [Test]
        public void Could_Replace_Detail_Summary()
        {
            const string readMeContent = @"
This is visible

<details>
<summary>Summary</summary>
This is retained
</details>

This is also visible
";

            var detailsStartRemoval = new RemovalOrReplacement(CommentOrRegex.Regex, "<details", ">", null);
            var detailsEndRemoval = new RemovalOrReplacement(CommentOrRegex.Regex, "</details", ">", null);
            var summaryRemoval = new RemovalOrReplacement(CommentOrRegex.Regex, "^<summary>.*?</", "summary>", null);
            var removeReplaceSettings = new RemoveReplaceSettings(null, [detailsStartRemoval, detailsEndRemoval, summaryRemoval], []);
            string rewrittenReadMe = _removeReplacer.RemoveReplace(readMeContent, removeReplaceSettings)!;

            const string expectedReadMeContent = @"
This is visible

This is retained

This is also visible
";

            Assert.That(rewrittenReadMe, Is.EqualTo(expectedReadMeContent));
        }
    }
}
