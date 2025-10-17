using System.Linq;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Markdig.Syntax;

namespace NugetRepoReadme.AngleSharpDom
{
    internal class HtmlFragmentParser : IHtmlFragmentParser
    {
        public INode Parse(HtmlBlock htmlBlock) => Parse(htmlBlock.Lines.ToString());

        public INode Parse(string fragment)
        {
            IBrowsingContext context = BrowsingContext.New(Configuration.Default);
            IHtmlParser? parser = context.GetService<IHtmlParser>();
            IDocument document = context.OpenNewAsync().Result;
            AngleSharp.Html.Dom.IHtmlElement? body = document.Body;
            INode root = parser!.ParseFragment(fragment, body!).First();
            return root;
        }
    }
}
