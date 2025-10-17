using System;

namespace NugetRepoReadme.NugetValidation
{
    // https://github.com/NuGet/NuGetGallery/blob/main/src/NuGetGallery/Services/ImageDomainValidator.cs
    internal class NuGetImageDomainValidator : INuGetImageDomainValidator
    {
        private readonly INuGetTrustedImageDomains _trustedImageDomains;
        private readonly INuGetGitHubBadgeValidator _nugetGitHubBadgeValidator;

        public NuGetImageDomainValidator(INuGetTrustedImageDomains trustedImageDomains, INuGetGitHubBadgeValidator nugetGitHubBadgeValidator)
        {
            _trustedImageDomains = trustedImageDomains;
            _nugetGitHubBadgeValidator = nugetGitHubBadgeValidator;
        }

        public bool IsTrustedImageDomain(string uriString)
            => Uri.TryCreate(uriString, UriKind.Absolute, out Uri? uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) && IsTrustedImageDomain(uri);

        private bool IsTrustedImageDomain(Uri uri) => _trustedImageDomains.IsImageDomainTrusted(uri.Host) ||
                _nugetGitHubBadgeValidator.Validate(uri.OriginalString);
    }
}
