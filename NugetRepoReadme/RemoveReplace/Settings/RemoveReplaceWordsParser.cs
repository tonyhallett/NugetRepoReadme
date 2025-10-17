using System.Collections.Generic;

namespace NugetRepoReadme.RemoveReplace.Settings
{
    internal static class RemoveReplaceWordsParser
    {
        /*
Removals
---
regex1
regex2

Replacements
---
regex1
replacement1
*/
        public static List<RemoveReplaceWord> Parse(string[] lines)
        {
            var removeReplaceWords = new List<RemoveReplaceWord>();
            const string removalsHeader = "Removals";
            const string replacementsHeader = "Replacements";
            bool completedRemovals = false;
            bool inRemovals = false;
            bool inReplacements = false;
            bool skipLine = false;

            int numLines = lines.Length;
            int lineNumber = -1;
            foreach (string line in lines)
            {
                lineNumber++;
                if (skipLine)
                {
                    skipLine = false;
                    continue;
                }

                if (!inRemovals && !inReplacements)
                {
                    if (!completedRemovals && LineIsHeader(removalsHeader))
                    {
                        inRemovals = true;
                    }
                    else if (LineIsHeader(replacementsHeader))
                    {
                        inReplacements = true;
                    }

                    continue;
                }

                if (inRemovals)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        completedRemovals = true;
                        inRemovals = false;
                        continue;
                    }

                    removeReplaceWords.Add(new RemoveReplaceWord(line, null));
                }

                if (inReplacements && !IsLastLine())
                {
                    skipLine = true;
                    string replacementLine = lines[lineNumber + 1];
                    removeReplaceWords.Add(new RemoveReplaceWord(line, replacementLine));
                }

                bool IsLastLine() => lineNumber + 1 >= numLines;

                bool LineIsHeader(string header)
                {
                    if (line.Trim() != header || IsLastLine() || !IsNextLineSeparator())
                    {
                        return false;
                    }

                    skipLine = true;
                    return true;
                }

                bool IsNextLineSeparator() => lines[lineNumber + 1] == "---";
            }

            return removeReplaceWords;
        }
    }
}
