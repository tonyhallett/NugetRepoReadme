namespace NugetRepoReadme.RemoveReplace.Settings
{
    internal interface IRemoveCommentsIdentifiersParser
    {
        RemoveCommentIdentifiers? Parse(string? removeCommentIdentifiers, IAddError addErrors);
    }
}
