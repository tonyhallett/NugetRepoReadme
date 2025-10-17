namespace NugetRepoReadme.NugetValidation
{
    internal interface INuGetImageDomainValidator
    {
        bool IsTrustedImageDomain(string uriString);
    }
}
