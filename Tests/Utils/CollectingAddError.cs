using NugetRepoReadme.RemoveReplace.Settings;

namespace Tests.Utils
{
    internal sealed class CollectingAddError : IAddError
    {
        public List<string> Errors { get; } = [];

        public void AddError(string message) => Errors.Add(message);

        public string Single() => Errors.Single();
    }
}
