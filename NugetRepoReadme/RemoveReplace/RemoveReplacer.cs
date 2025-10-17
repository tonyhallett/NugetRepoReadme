using NugetRepoReadme.RegExp;
using NugetRepoReadme.RemoveReplace.Settings;

namespace NugetRepoReadme.RemoveReplace
{
    internal class RemoveReplacer : IRemoveReplacer
    {
        private readonly IRemoveReplaceRegexesFactory _removeReplaceRegexesFactory;

        public RemoveReplacer(IRemoveReplaceRegexesFactory removeReplaceRegexesFactory)
            => _removeReplaceRegexesFactory = removeReplaceRegexesFactory;

        private string[] GetLines(string input)
        {
            // Normalize line endings so splitting is predictable
            string normalized = input.Replace("\r\n", "\n");

            // Keep empty entries (we want to preserve blank lines)
            return normalized.Split('\n');
        }

        public string RemoveReplace(string input, RemoveReplaceSettings settings)
        {
            IRemoveReplaceRegexes removeReplaceRegexes = _removeReplaceRegexesFactory.Create(settings);
            if (!removeReplaceRegexes.Any)
            {
                return input;
            }

            var lineBuilder = new LineBuilder();
            bool inRemovalReplacement = false;
            string replacementText = string.Empty;
            string[] lines = GetLines(input);

            for (int i = 0; i < lines.Length; i++)
            {
                bool isLast = i == lines.Length - 1;
                string line = lines[i];
                string? replacedLine = removeReplaceRegexes.ReplaceWords(line);
                bool didReplaceLine = replacedLine != null;
                if (replacedLine != null)
                {
                    line = replacedLine;
                }

                if (!inRemovalReplacement)
                {
                    MatchStartResult matchStartResult = removeReplaceRegexes.MatchStart(line);
                    System.Text.RegularExpressions.Match startMatch = matchStartResult.Match;
                    if (!startMatch.Success)
                    {
                        if (didReplaceLine)
                        {
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                lineBuilder.AppendLine(line, isLast);
                            }
                        }
                        else
                        {
                            // No start marker -> keep whole line
                            lineBuilder.AppendLine(line, isLast);
                        }

                        continue;
                    }

                    replacementText = matchStartResult.ReplacementText ?? string.Empty;
                    string before = startMatch.Before(line);
                    if (matchStartResult.IsRemaining)
                    {
                        lineBuilder.AppendLine(before + replacementText, true);
                        break;
                    }

                    string afterStart = startMatch.After(line);

                    // Maybe the end marker is on the same line (start and end on one line)
                    System.Text.RegularExpressions.Match endMatchInSameLine = removeReplaceRegexes.MatchEnd(afterStart);
                    if (endMatchInSameLine.Success)
                    {
                        // Keep text before the start marker + text after the end marker
                        string afterEnd = endMatchInSameLine.After(afterStart);
                        string merged = before + replacementText + afterEnd;

                        // Add merged only if not empty (this preserves inline non-comment snippets)
                        if (!string.IsNullOrWhiteSpace(merged))
                        {
                            lineBuilder.AppendLine(merged, isLast);
                        }
                    }
                    else
                    {
                        // Start marker without end on same line.
                        // Preserve text before the start marker (if any), start removing from now on.
                        if (!string.IsNullOrWhiteSpace(before))
                        {
                            lineBuilder.AppendLine(before, isLast); // preserve inline "left" content
                        }

                        inRemovalReplacement = true;
                    }
                }
                else
                {
                    // We are inside a removal block: look for end marker on this line
                    System.Text.RegularExpressions.Match endMatch = removeReplaceRegexes.MatchEnd(line);
                    if (!endMatch.Success)
                    {
                        // No end marker -> drop the whole line
                        continue;
                    }

                    // Found end marker. Preserve anything after the end marker.
                    string after = endMatch.After(line);
                    string appendText = replacementText + after;
                    if (!string.IsNullOrWhiteSpace(appendText))
                    {
                        lineBuilder.AppendLine(appendText, isLast);
                    }

                    inRemovalReplacement = false;
                }
            }

            return lineBuilder.ToString();
        }
    }
}
