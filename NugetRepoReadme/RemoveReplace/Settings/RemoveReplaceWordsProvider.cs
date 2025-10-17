using System.Collections.Generic;
using Microsoft.Build.Framework;
using NugetRepoReadme.IOWrapper;
using NugetRepoReadme.MSBuild;

namespace NugetRepoReadme.RemoveReplace.Settings
{
    internal class RemoveReplaceWordsProvider : IRemoveReplaceWordsProvider
    {
        private readonly IIOHelper _ioHelper;
        private readonly IMessageProvider _messageProvider;

        public RemoveReplaceWordsProvider(IIOHelper ioHelper, IMessageProvider instance)
        {
            _ioHelper = ioHelper;
            _messageProvider = instance;
        }

        public List<RemoveReplaceWord> Provide(ITaskItem[]? removeReplaceWordsItems, IAddError addError)
        {
            var removeReplaceWords = new List<RemoveReplaceWord>();
            if (removeReplaceWordsItems == null)
            {
                return removeReplaceWords;
            }

            foreach (ITaskItem removeReplaceWordsItem in removeReplaceWordsItems)
            {
                string filePath = removeReplaceWordsItem.ItemSpec;
                if (!_ioHelper.FileExists(filePath))
                {
                    string errorMessage = _messageProvider.RemoveReplaceWordsFileDoesNotExist(filePath);
                    addError.AddError(errorMessage);
                }

                string[] lines = _ioHelper.ReadAllLines(filePath);
                removeReplaceWords.AddRange(RemoveReplaceWordsParser.Parse(lines));
            }

            return removeReplaceWords;
        }
    }
}
