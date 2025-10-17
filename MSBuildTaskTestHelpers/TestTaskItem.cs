using System.Collections;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MSBuildTaskTestHelpers
{
    /// <summary>
    /// Wraps TaskItem to allow setting ItemSpec modifiers.
    /// </summary>
    public class TestTaskItem : ITaskItem2
    {
        private readonly TaskItem _taskItem;

        private ITaskItem2 TaskItem2 => _taskItem;

        private readonly Dictionary<string, Func<string?>>? _metadataFromItemSpecModifiers;

        public TestTaskItem(Dictionary<string, string>? metadata = null, string itemSpec = "itemspec", ItemSpecModifiersMetadata? itemSpecModifiers = null)
        {
            metadata ??= [];
            if (itemSpecModifiers != null)
            {
                _metadataFromItemSpecModifiers = new Dictionary<string, Func<string?>>(StringComparer.OrdinalIgnoreCase)
                {
                    { nameof(ItemSpecModifiersMetadata.FullPath), () => itemSpecModifiers.FullPath },
                    { nameof(ItemSpecModifiersMetadata.RootDir), () => itemSpecModifiers.RootDir },
                    { nameof(ItemSpecModifiersMetadata.FileName), () => itemSpecModifiers.FileName },
                    { nameof(ItemSpecModifiersMetadata.Extension), () => itemSpecModifiers.Extension },
                    { nameof(ItemSpecModifiersMetadata.Directory), () => itemSpecModifiers.Directory },
                    { nameof(ItemSpecModifiersMetadata.RelativeDir), () => itemSpecModifiers.RelativeDir },
                    { nameof(ItemSpecModifiersMetadata.ModifiedTime), itemSpecModifiers.TryGetModifiedTime },
                    { nameof(ItemSpecModifiersMetadata.CreatedTime), itemSpecModifiers.TryGetCreatedTime },
                    { nameof(ItemSpecModifiersMetadata.AccessedTime), itemSpecModifiers.TryGetAccessedTime },
                    { nameof(ItemSpecModifiersMetadata.DefiningProjectFullPath), () => itemSpecModifiers.DefiningProjectFullPath },
                    { nameof(ItemSpecModifiersMetadata.DefiningProjectDirectory), () => itemSpecModifiers.DefiningProjectDirectory },
                    { nameof(ItemSpecModifiersMetadata.DefiningProjectName), () => itemSpecModifiers.DefiningProjectName },
                    { nameof(ItemSpecModifiersMetadata.DefiningProjectExtension), () => itemSpecModifiers.DefiningProjectExtension },
                };
            }

            _taskItem = new TaskItem(itemSpec, metadata);
        }

        public string ItemSpec
        {
            get => _taskItem.ItemSpec;
            set => _taskItem.ItemSpec = value;
        }

        public ICollection MetadataNames => _taskItem.MetadataNames;

        public int MetadataCount => _taskItem.MetadataCount;

        public string EvaluatedIncludeEscaped
        {
            get => TaskItem2.EvaluatedIncludeEscaped;
            set => TaskItem2.EvaluatedIncludeEscaped = value;
        }

        public IDictionary CloneCustomMetadata() => _taskItem.CloneCustomMetadata();

        public IDictionary CloneCustomMetadataEscaped() => TaskItem2.CloneCustomMetadataEscaped();

        public void CopyMetadataTo(ITaskItem destinationItem) => _taskItem.CopyMetadataTo(destinationItem);

        // TaskItem
        /*
            public string GetMetadata(string metadataName)
            {
                string metadataValue = (this as ITaskItem2).GetMetadataValueEscaped(metadataName);
                return EscapingUtilities.UnescapeAll(metadataValue);
            }
        */
        public string GetMetadata(string metadataName)
        {
            string? fromModifiers = TryGetFromItemSpecModifiers(metadataName);
            return fromModifiers ?? _taskItem.GetMetadata(metadataName);
        }

        public string GetMetadataValueEscaped(string metadataName)
        {
            string? fromModifiers = TryGetFromItemSpecModifiers(metadataName);
            return fromModifiers ?? TaskItem2.GetMetadataValueEscaped(metadataName);
        }

        private string? TryGetFromItemSpecModifiers(string metadataName) => _metadataFromItemSpecModifiers == null
                ? null
                : _metadataFromItemSpecModifiers.TryGetValue(metadataName, out Func<string?>? func) ? func() : null;

        public void RemoveMetadata(string metadataName) => _taskItem.RemoveMetadata(metadataName);

        public void SetMetadata(string metadataName, string metadataValue) => _taskItem.SetMetadata(metadataName, metadataValue);

        public void SetMetadataValueLiteral(string metadataName, string metadataValue) => TaskItem2.SetMetadataValueLiteral(metadataName, metadataValue);

    }
}
