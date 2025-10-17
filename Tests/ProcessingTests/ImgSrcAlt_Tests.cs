﻿using AngleSharp.Html.Dom;
using Moq;
using NugetRepoReadme.Processing;

namespace Tests.ProcessingTests
{
    internal sealed class ImgSrcAlt_Tests
    {
        [Test]
        public void Should_Be_Null_When_No_Src()
        {
            var mockHtmlImageElement = new Mock<IHtmlImageElement>();
            _ = mockHtmlImageElement.Setup(ie => ie.GetAttribute("alt")).Returns((string?)null);

            Assert.That(ImgSrcAlt.TryGet(mockHtmlImageElement.Object), Is.Null);
        }

        [Test]
        public void Should_Have_Empty_Alt_When_No_Alt_Attribute()
        {
            var mockHtmlImageElement = new Mock<IHtmlImageElement>();
            _ = mockHtmlImageElement.Setup(ie => ie.GetAttribute("src")).Returns("src");

            Assert.That(ImgSrcAlt.TryGet(mockHtmlImageElement.Object)!.Alt, Is.Empty);
        }

        [Test]
        public void Should_Have_Attributes_Alt_Src()
        {
            var mockHtmlImageElement = new Mock<IHtmlImageElement>();
            _ = mockHtmlImageElement.Setup(ie => ie.GetAttribute("src")).Returns("src");
            _ = mockHtmlImageElement.Setup(ie => ie.GetAttribute("alt")).Returns("alt");

            ImgSrcAlt imgSrcAlt = ImgSrcAlt.TryGet(mockHtmlImageElement.Object)!;

            Assert.Multiple(() =>
            {
                Assert.That(imgSrcAlt.Alt, Is.EqualTo("alt"));
                Assert.That(imgSrcAlt.Src, Is.EqualTo("src"));
            });
        }
    }
}
