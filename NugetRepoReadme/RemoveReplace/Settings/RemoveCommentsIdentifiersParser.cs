using NugetRepoReadme.MSBuild;

namespace NugetRepoReadme.RemoveReplace.Settings
{
    internal class RemoveCommentsIdentifiersParser : IRemoveCommentsIdentifiersParser
    {
        private readonly IMessageProvider _messageProvider;

        public RemoveCommentsIdentifiersParser(IMessageProvider messageProvider) => _messageProvider = messageProvider;

        public RemoveCommentIdentifiers? Parse(string? removeCommentIdentifiers, IAddError addError)
        {
            if (string.IsNullOrEmpty(removeCommentIdentifiers))
            {
                return null;
            }

            string[] parts = removeCommentIdentifiers!.Split(';');
            if (parts.Length > 2)
            {
                addError.AddError(_messageProvider.RemoveCommentsIdentifiersFormat());
                return null;
            }

            string start = parts[0].Trim();
            string? end = null;
            if (parts.Length == 2)
            {
                end = parts[1].Trim();
            }

            if (string.IsNullOrEmpty(start))
            {
                addError.AddError(_messageProvider.RemoveCommentsIdentifiersFormat());
                return null;
            }

            if (start == end)
            {
                addError.AddError(_messageProvider.RemoveCommentsIdentifiersSameStartEnd());
                return null;
            }

            return new RemoveCommentIdentifiers(start, end);
        }
    }
}
