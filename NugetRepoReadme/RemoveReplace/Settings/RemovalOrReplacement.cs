﻿namespace NugetRepoReadme.RemoveReplace.Settings
{
    internal class RemovalOrReplacement
    {
        public RemovalOrReplacement(
            CommentOrRegex commentOrRegex,
            string start,
            string? end,
            string? replacementText)
        {
            CommentOrRegex = commentOrRegex;
            Start = start;
            End = end;
            ReplacementText = replacementText;
        }

        public CommentOrRegex CommentOrRegex { get; }

        public string Start { get; }

        public string? End { get; }

        public string? ReplacementText { get; set; }
    }
}
