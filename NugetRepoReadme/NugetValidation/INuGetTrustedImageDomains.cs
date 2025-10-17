namespace NugetRepoReadme.NugetValidation
{
    internal interface INuGetTrustedImageDomains
    {
        bool IsImageDomainTrusted(string imageDomain);
    }
}
