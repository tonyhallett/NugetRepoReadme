using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Framework;
using NugetRepoReadme.MSBuild;
using NugetRepoReadme.MSBuildHelpers;

namespace NugetRepoReadme.RemoveReplace.Settings
{
    internal class RemoveReplaceSettingsProvider : IRemoveReplaceSettingsProvider
    {
        private readonly IMSBuildMetadataProvider _msBuildMetadataProvider;
        private readonly IRemoveCommentsIdentifiersParser _removeCommentsIdentifiersParser;
        private readonly IRemovalOrReplacementProvider _removalOrReplacementProvider;
        private readonly IRemoveReplaceWordsProvider _removeReplaceWordsProvider;
        private readonly IMessageProvider _messageProvider;

        public RemoveReplaceSettingsProvider(
            IMSBuildMetadataProvider msBuildMetadataProvider,
            IRemoveCommentsIdentifiersParser removeCommentsIdentifiersParser,
            IRemovalOrReplacementProvider removalOrReplacementProvider,
            IRemoveReplaceWordsProvider removeReplaceWordsProvider,
            IMessageProvider messageProvider)
        {
            _msBuildMetadataProvider = msBuildMetadataProvider;
            _removeCommentsIdentifiersParser = removeCommentsIdentifiersParser;
            _removalOrReplacementProvider = removalOrReplacementProvider;
            _removeReplaceWordsProvider = removeReplaceWordsProvider;
            _messageProvider = messageProvider;
        }

        public IRemoveReplaceSettingsResult Provide(
            ITaskItem[]? removeReplaceItems,
            ITaskItem[]? removeReplaceWordsItems,
            string? removeCommentIdentifiers)
        {
            var removeReplaceSettingsResult = new RemoveReplaceSettingsResult();
            RemoveCommentIdentifiers? parsedRemoveCommentIdentifiers = _removeCommentsIdentifiersParser.Parse(removeCommentIdentifiers, removeReplaceSettingsResult);
            List<RemovalOrReplacement> removalOrReplacements = CreateRemovalOrReplacements(removeReplaceItems, removeReplaceSettingsResult);
            List<RemoveReplaceWord> removeReplaceWords = _removeReplaceWordsProvider.Provide(removeReplaceWordsItems, removeReplaceSettingsResult);

            // if errors are added to and checked then this should not be necessary.
            if (removeReplaceSettingsResult.Errors.Count > 0 || (parsedRemoveCommentIdentifiers == null && removalOrReplacements.Count == 0 && removeReplaceWords.Count == 0))
            {
                return removeReplaceSettingsResult;
            }

            removeReplaceSettingsResult.Settings = new RemoveReplaceSettings(
                parsedRemoveCommentIdentifiers, removalOrReplacements, removeReplaceWords);
            return removeReplaceSettingsResult;
        }

        private List<RemovalOrReplacement> CreateRemovalOrReplacements(
            ITaskItem[]? removeReplaceItems,
            IAddError addError)
        {
            var removalReplacements = new List<RemovalOrReplacement>();
            List<MetadataItem> metadataItemsWithoutMissingMetadata = GetMetadataItemsWithoutMissingMetadata(removeReplaceItems, addError);

            foreach (MetadataItem metadataItemWithoutMissingMetadata in metadataItemsWithoutMissingMetadata)
            {
                RemovalOrReplacement? removalOrReplacement = _removalOrReplacementProvider.Provide(metadataItemWithoutMissingMetadata, addError);
                if (removalOrReplacement != null)
                {
                    removalReplacements.Add(removalOrReplacement);
                }
            }

            return removalReplacements.Count == removeReplaceItems?.Length ? removalReplacements : Enumerable.Empty<RemovalOrReplacement>().ToList();
        }

        private List<MetadataItem> GetMetadataItemsWithoutMissingMetadata(ITaskItem[]? removeReplaceItems, IAddError addError)
        {
            List<MetadataItem> metadataItems = new List<MetadataItem>();
            if (removeReplaceItems == null)
            {
                return metadataItems;
            }

            foreach (ITaskItem removeReplaceItem in removeReplaceItems)
            {
                RemoveReplaceMetadata metadata = _msBuildMetadataProvider.GetCustomMetadata<RemoveReplaceMetadata>(removeReplaceItem);
                if (metadata.MissingMetadataNames.Count > 0)
                {
                    foreach (string missingMetadataName in metadata.MissingMetadataNames)
                    {
                        addError.AddError(_messageProvider.RequiredMetadata(
                            missingMetadataName,
                            removeReplaceItem.ItemSpec));
                    }
                }
                else
                {
                    metadataItems.Add(new MetadataItem(metadata, removeReplaceItem));
                }
            }

            return metadataItems;
        }
    }
}
