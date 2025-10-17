using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace NugetRepoReadme.NugetValidation
{
    [ExcludeFromCodeCoverage]
    internal class NuGetGitHubBadgeValidator : INuGetGitHubBadgeValidator
    {
        private static readonly TimeSpan s_regexTimeout = TimeSpan.FromMinutes(1);

        private static readonly Regex s_gitHubBadgeUrlRegEx = new Regex("^(https|http):\\/\\/github\\.com\\/[^/]+\\/[^/]+(\\/actions)?\\/workflows\\/.*badge\\.svg", RegexOptions.IgnoreCase, s_regexTimeout);

        public bool Validate(string url)
        {
            try
            {
                return s_gitHubBadgeUrlRegEx.IsMatch(url);
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
    }
}
