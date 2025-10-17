namespace NugetRepoReadme.MarkdigHelpers
{
    internal class HtmlTagInfo
    {
        public HtmlTagInfo(string tagName, bool isStart)
        {
            TagName = tagName;
            IsStart = isStart;
        }

        public string TagName { get; }

        public bool IsStart { get; }
    }
}
