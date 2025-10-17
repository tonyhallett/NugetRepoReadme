namespace NugetRepoReadme.RemoveReplace.Settings
{
    internal class RemoveCommentIdentifiers
    {
        public RemoveCommentIdentifiers(string startCommentIdentifier, string? endCommentIdentifier)
        {
            Start = startCommentIdentifier;
            End = endCommentIdentifier;
        }

        public string Start { get; }

        public string? End { get; }
    }
}
