using Markdig.Syntax;
using NugetRepoReadme.ReadmeReplacement;

namespace Tests.ReadmeReplacementTests
{
    internal sealed class ReplacementResult_Tests
    {
        [Test]
        public void Should_Use_SourceReplacement()
        {
            var replacementResult = new ReplacementResult(
                "replace this with that",
                [
                    new(new SourceSpan(8, 11), "that"),
                    new(new SourceSpan(18, 21), "this"),
                ]);

            Assert.That(replacementResult.Result, Is.EqualTo("replace that with this"));
        }

        [Test]
        public void Should_Use_Further_Replacement()
        {
            var replacementResult = new ReplacementResult(
                "replace this further",
                [
                    new(new SourceSpan(8, 11), "first second third", true),
                ]);
            replacementResult.FurtherReplacements.First().SetSourceReplacements(
                [
                    new(new SourceSpan(6, 11), "replaced"),
                ]);
            replacementResult.ApplyFurtherReplacements();

            Assert.That(replacementResult.Result, Is.EqualTo("replace first replaced third further"));
        }

        [Test]
        public void Should_Use_Further_Replacement_Empty()
        {
            var replacementResult = new ReplacementResult(
                "replace this further",
                [
                    new(new SourceSpan(8, 11), "first second third", true),
                ]);

            replacementResult.ApplyFurtherReplacements();

            Assert.That(replacementResult.Result, Is.EqualTo("replace first second third further"));
        }
    }
}
