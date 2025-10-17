using System.Collections.Generic;

namespace NugetRepoReadme.RemoveReplace.Settings
{
    internal class RemoveReplaceSettings
    {
        public RemoveReplaceSettings(
            RemoveCommentIdentifiers? removeCommentIdentifiers,
            List<RemovalOrReplacement> removalsOrReplacements,
            List<RemoveReplaceWord> removeReplaceWords)
        {
            RemoveCommentIdentifiers = removeCommentIdentifiers;
            RemovalsOrReplacements = removalsOrReplacements;
            RemoveReplaceWords = removeReplaceWords;
        }

        public RemoveCommentIdentifiers? RemoveCommentIdentifiers { get; }

        public List<RemovalOrReplacement> RemovalsOrReplacements { get; }

        public List<RemoveReplaceWord> RemoveReplaceWords { get; }
    }
}
