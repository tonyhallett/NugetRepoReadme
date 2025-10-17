namespace NugetRepoReadme.RemoveReplace.Settings
{
    internal interface IRemovalOrReplacementProvider
    {
        RemovalOrReplacement? Provide(MetadataItem metadataItem, IAddError addError);
    }
}
