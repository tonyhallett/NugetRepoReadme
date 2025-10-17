﻿using Moq;
using NugetRepoReadme.NugetValidation;

namespace Tests.NugetValidationTests
{
    internal sealed class NuGetImageDomainValidator_Tests
    {
        private Mock<INuGetTrustedImageDomains> _mockNuGetTrustedImageDomains = new();
        private Mock<INuGetGitHubBadgeValidator> _mockNuGetGitHubBadgeValidator = new();
        private NuGetImageDomainValidator? _nuGetImageDomainValidator;

        [SetUp]
        public void SetUp()
        {
            _mockNuGetTrustedImageDomains = new Mock<INuGetTrustedImageDomains>();
            _mockNuGetGitHubBadgeValidator = new Mock<INuGetGitHubBadgeValidator>();
            _nuGetImageDomainValidator = new NuGetImageDomainValidator(_mockNuGetTrustedImageDomains.Object, _mockNuGetGitHubBadgeValidator.Object);
        }

        [Test]
        public void Should_Not_Be_Trusted_If_Relative_Url()
        {
            Assert.That(_nuGetImageDomainValidator!.IsTrustedImageDomain("/relative"), Is.False);
            VerifyDependencies(false);
        }

        [Test]
        public void Should_Not_Be_Trusted_For_Non_Http_Or_Https()
        {
            const string ftpUrl = "ftp://example.com/image.png";
            Assert.That(_nuGetImageDomainValidator!.IsTrustedImageDomain(ftpUrl), Is.False);
            VerifyDependencies(false);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Be_Trusted_If_The_Host_Is_A_Trusted_Domain(bool https)
        {
            string protocol = https ? "https" : "http";
            _ = _mockNuGetTrustedImageDomains.Setup(nugetTrustedImageDomains => nugetTrustedImageDomains.IsImageDomainTrusted("trusted.com")).Returns(true);
            Assert.That(_nuGetImageDomainValidator!.IsTrustedImageDomain($"{protocol}://trusted.com/page"), Is.True);
        }

        [Test]
        public void Should_Be_Trusted_If_The_Url_Is_A_Valid_GitHub_Badge()
        {
            const string validBadgeUrl = "https://validbadge";
            _ = _mockNuGetGitHubBadgeValidator.Setup(nugetGitHubBadgeValidator => nugetGitHubBadgeValidator.Validate(validBadgeUrl)).Returns(true);
            Assert.That(_nuGetImageDomainValidator!.IsTrustedImageDomain(validBadgeUrl), Is.True);
        }

        [Test]
        public void Should_Not_Be_Trusted_If_Is_Neither_A_Trusted_Domain_Or_GitHub_Badge_Url()
        {
            Assert.That(_nuGetImageDomainValidator!.IsTrustedImageDomain("https://example.com/page"), Is.False);
            VerifyDependencies(true);
        }

        private void VerifyDependencies(bool called)
        {
            Times times = called ? Times.Once() : Times.Never();
            _mockNuGetTrustedImageDomains.Verify(x => x.IsImageDomainTrusted(It.IsAny<string>()), times);
            _mockNuGetGitHubBadgeValidator.Verify(x => x.Validate(It.IsAny<string>()), times);
        }
    }
}
