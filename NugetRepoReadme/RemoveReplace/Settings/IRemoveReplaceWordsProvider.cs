using System.Collections.Generic;
using Microsoft.Build.Framework;

namespace NugetRepoReadme.RemoveReplace.Settings
{
    internal interface IRemoveReplaceWordsProvider
    {
        List<RemoveReplaceWord> Provide(ITaskItem[]? removeReplaceWordsItems, IAddError addError);
    }
}
