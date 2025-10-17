using System;
using System.Collections.Generic;
using Markdig.Syntax;
using NugetRepoReadme.ReadmeReplacement;

namespace NugetRepoReadme.Processing
{
    internal class MarkdownElementsProcessResult : IMarkdownElementsProcessResult
    {
        private readonly List<SourceReplacement> _sourceReplacements = new List<SourceReplacement>();
        private readonly HashSet<string> _unsupportedImageDomains = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _missingReadmeAssets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public IEnumerable<string> UnsupportedImageDomains => _unsupportedImageDomains;

        public IEnumerable<string> MissingReadmeAssets => _missingReadmeAssets;

        public IEnumerable<SourceReplacement> SourceReplacements => _sourceReplacements;

        public bool HasUnsupportedHtml { get; internal set; }

        public void AddSourceReplacement(SourceSpan sourceSpan, string replacement, bool furtherProcessingRequired = false)
            => _sourceReplacements.Add(new SourceReplacement(sourceSpan, replacement, furtherProcessingRequired));

        public void AddUnsupportedImageDomain(string domain) => _unsupportedImageDomains.Add(domain);

        public void CombineIssues(IMarkdownElementsProcessResult next)
        {
            _unsupportedImageDomains.UnionWith(next.UnsupportedImageDomains);
            _missingReadmeAssets.UnionWith(next.MissingReadmeAssets);
            if (!next.HasUnsupportedHtml)
            {
                return;
            }

            HasUnsupportedHtml = true;
        }

        internal void AddMissingReadmeAsset(string url) => _missingReadmeAssets.Add(url);
    }
}
