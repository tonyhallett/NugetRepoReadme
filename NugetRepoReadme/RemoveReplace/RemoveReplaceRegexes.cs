using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NugetRepoReadme.RemoveReplace
{
    internal class RemoveReplaceRegexes : IRemoveReplaceRegexes
    {
        private readonly RemoveCommentRegexes? _removeCommentRegexes;
        private readonly List<RemoveReplaceWordRegex> _removeReplaceWords;
        private readonly List<RegexRemovalOrReplacement> _regexRemovalOrReplacements;
        private bool _matchedRemoveCommentRegexes;
        private RegexRemovalOrReplacement? _matchedRegexRemovalOrReplacement;

        public RemoveReplaceRegexes(
            List<RegexRemovalOrReplacement> regexRemovalOrReplacements,
            RemoveCommentRegexes? removeCommentRegexes,
            List<RemoveReplaceWordRegex> removeReplaceWords)
        {
            _regexRemovalOrReplacements = regexRemovalOrReplacements;
            _removeCommentRegexes = removeCommentRegexes;
            _removeReplaceWords = removeReplaceWords;
        }

        public bool Any => _removeCommentRegexes != null || _regexRemovalOrReplacements.Count > 0 || _removeReplaceWords.Count > 0;

        public MatchStartResult MatchStart(string text)
        {
            Match match = Match.Empty;
            if (_removeCommentRegexes != null)
            {
                match = _removeCommentRegexes.StartRegex.Match(text);
                if (match.Success)
                {
                    _matchedRemoveCommentRegexes = true;
                    return new MatchStartResult(match, _removeCommentRegexes.EndRegex == null, null);
                }
            }

            foreach (RegexRemovalOrReplacement regexRemovalOrReplacement in _regexRemovalOrReplacements)
            {
                match = regexRemovalOrReplacement.StartRegex.Match(text);
                if (match.Success)
                {
                    if (regexRemovalOrReplacement.EndRegex != null)
                    {
                        _matchedRegexRemovalOrReplacement = regexRemovalOrReplacement;
                    }

                    return new MatchStartResult(match, regexRemovalOrReplacement.EndRegex == null, regexRemovalOrReplacement.ReplacementText);
                }
            }

            return new MatchStartResult(match);
        }

        public Match MatchEnd(string text)
        {
            Match match;
            if (_matchedRemoveCommentRegexes)
            {
                match = _removeCommentRegexes!.EndRegex!.Match(text);
                if (match.Success)
                {
                    _matchedRemoveCommentRegexes = false;
                }

                return match;
            }

            match = _matchedRegexRemovalOrReplacement!.EndRegex!.Match(text);
            if (match.Success)
            {
                _matchedRegexRemovalOrReplacement = null;
            }

            return match;
        }

        public string? ReplaceWords(string line)
        {
            bool anyReplaced = false;
            string result = line;
            foreach (RemoveReplaceWordRegex removeReplaceWords in _removeReplaceWords)
            {
                result = removeReplaceWords.Regex.Replace(result, _ =>
                {
                    anyReplaced = true;
                    return removeReplaceWords.Replacement;
                });
            }

            return anyReplaced ? result : null;
        }
    }
}
