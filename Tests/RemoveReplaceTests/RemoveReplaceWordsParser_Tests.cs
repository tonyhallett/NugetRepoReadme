using NugetRepoReadme.RemoveReplace.Settings;

namespace Tests.RemoveReplaceTests
{
    internal sealed class RemoveReplaceWordsParser_Tests
    {
        [Test]
        public void Should_Parse_Removals()
        {
            string[] lines =
            [
                "Removals",
                "---",
                "word1",
                "word2",
                "word3",
            ];

            List<RemoveReplaceWord> parsed = RemoveReplaceWordsParser.Parse(lines);

            Assert.That(
                parsed,
                Is.EqualTo(new[]
                {
                    new RemoveReplaceWord("word1", null),
                    new RemoveReplaceWord("word2", null),
                    new RemoveReplaceWord("word3", null),
                }));
        }

        [Test]
        public void Should_Parse_Removals_Blank_Line_Preceeding_Header()
        {
            string[] lines =
            [
                string.Empty,
                "Removals",
                "---",
                "word1",
                "word2",
                "word3",
            ];

            List<RemoveReplaceWord> parsed = RemoveReplaceWordsParser.Parse(lines);

            Assert.That(
                parsed,
                Is.EqualTo(new[]
                {
                    new RemoveReplaceWord("word1", null),
                    new RemoveReplaceWord("word2", null),
                    new RemoveReplaceWord("word3", null),
                }));
        }

        [Test]
        public void Should_Parse_Replacements()
        {
            string[] lines =
            [
                "Replacements",
                "---",
                "word1",
                "replacement1",
                "word2",
                "replacement2",
            ];

            List<RemoveReplaceWord> parsed = RemoveReplaceWordsParser.Parse(lines);

            Assert.That(
                parsed,
                Is.EqualTo(new[]
                {
                    new RemoveReplaceWord("word1", "replacement1"),
                    new RemoveReplaceWord("word2", "replacement2"),
                }));
        }

        [Test]
        public void Should_Parse_Should_Parse_Removals_And_Replacements()
        {
            string[] lines =
            [
                "Removals",
                "---",
                "word1",
                "word2",
                "word3",
                string.Empty,
                "Replacements",
                "---",
                "word4",
                "replacement1",
                "word5",
                "replacement2",
            ];

            List<RemoveReplaceWord> parsed = RemoveReplaceWordsParser.Parse(lines);

            Assert.That(
                parsed,
                Is.EqualTo(new[]
                {
                    new RemoveReplaceWord("word1", null),
                    new RemoveReplaceWord("word2", null),
                    new RemoveReplaceWord("word3", null),
                    new RemoveReplaceWord("word4", "replacement1"),
                    new RemoveReplaceWord("word5", "replacement2"),
                }));
        }

        [Test]
        public void Should_Allow_Removal_Of_Removals()
        {
            string[] lines =
            [
                "Removals",
                "---",
                "Removals",
                "---",
            ];

            List<RemoveReplaceWord> parsed = RemoveReplaceWordsParser.Parse(lines);

            Assert.That(
                parsed,
                Is.EqualTo(new[]
                {
                    new RemoveReplaceWord("Removals", null),
                    new RemoveReplaceWord("---", null),
                }));
        }

        [Test]
        public void Should_Ignore_Missing_Replacement()
        {
            string[] lines =
            [
                "Replacements",
                "---",
                "word1",
                "replacement1",
                "word2", // missing replacement
            ];

            List<RemoveReplaceWord> parsed = RemoveReplaceWordsParser.Parse(lines);

            Assert.That(
                parsed,
                Is.EqualTo(new[]
                {
                    new RemoveReplaceWord("word1", "replacement1"),
                }));
        }
    }
}
