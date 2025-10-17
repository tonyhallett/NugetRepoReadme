using AngleSharp.Dom;
using Markdig.Syntax;

namespace NugetRepoReadme.AngleSharpDom
{
    internal interface IHtmlFragmentParser
    {
        INode Parse(HtmlBlock htmlBlock);

        INode Parse(string fragment);
    }
}
