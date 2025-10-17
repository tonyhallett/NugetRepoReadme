using Markdig.Syntax;

namespace NugetRepoReadme.MarkdigHelpers
{
    internal static class BlockExtensions
    {
        public static int NewLineLength(this Block block) => (int)block.NewLine & 0x03;
    }
}
