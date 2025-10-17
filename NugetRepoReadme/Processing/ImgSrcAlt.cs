using AngleSharp.Html.Dom;

namespace NugetRepoReadme.Processing
{
    internal class ImgSrcAlt
    {
        public ImgSrcAlt(string src, string? alt)
        {
            Src = src;
            Alt = alt ?? string.Empty;
        }

        public static ImgSrcAlt? TryGet(IHtmlImageElement imgElement)
        {
            string? src = imgElement.GetAttribute("src");
            string? alt = imgElement.GetAttribute("alt");
            return !string.IsNullOrWhiteSpace(src) ? new ImgSrcAlt(src!, alt) : null;
        }

        public string Src { get; }

        public string Alt { get; }
    }
}
