namespace NugetRepoReadme
{
    internal static class IsDebug
    {
        public static bool Value() =>
#if DEBUG
            true;
#else
            false;
#endif

    }
}
