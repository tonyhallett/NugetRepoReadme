using System;

namespace NugetRepoReadme.Repo
{
    internal class RepoUrlHelper : IRepoUrlHelper
    {
        public static RepoUrlHelper Instance { get; } = new RepoUrlHelper();

        public string? GetRepoAbsoluteUrl(
            string? url,
            RepoPaths repoPaths,
            bool isImage)
        {
            if (url == null || IsAbsolute(url))
            {
                return null;
            }

            url = url.Trim();

            string urlWithoutPath = isImage ? repoPaths.ImageBasePath : repoPaths.LinkBasePath;

            // repo relative
            if (url.StartsWith("/"))
            {
                return $"{urlWithoutPath}{url}";
            }

            // readme directory relative
            string readmeRelativePath = repoPaths.ReadmeRelativePath;
            if (!readmeRelativePath.StartsWith("/"))
            {
                readmeRelativePath = "/" + readmeRelativePath;
            }

            var readmeUri = new Uri(urlWithoutPath + readmeRelativePath);
            return new Uri(readmeUri, url).OriginalString;
        }

        public string GetAbsoluteOrRepoAbsoluteUrl(string url, RepoPaths repoPaths, bool isImage)
            => IsAbsolute(url) ? url! : GetRepoAbsoluteUrl(url, repoPaths, isImage)!;

        public Uri? GetAbsoluteUri(string? url)
            => url == null ? null : Uri.TryCreate(url, UriKind.Absolute, out Uri? absoluteUri) ? absoluteUri : null;

        private bool IsAbsolute(string? url) => GetAbsoluteUri(url) != null;
    }
}
