using System.IO.Compression;

namespace EndToEndTests
{
    internal static class NupkgReadmeReader
    {
        public static string Read(DirectoryInfo directoryInfo, string zipEntryName)
        {
            string dependentNuGetPath = GetDependentNuGetPath(directoryInfo);
            using ZipArchive zip = ZipFile.OpenRead(dependentNuGetPath);

            // nuget always stores readme at the root of the package
            ZipArchiveEntry? entry = zip.GetEntry(zipEntryName);

            using var reader = new StreamReader(entry!.Open());
            return reader.ReadToEnd();
        }

        private static string GetDependentNuGetPath(DirectoryInfo directoryInfo) => directoryInfo.GetFiles("*.nupkg", SearchOption.AllDirectories).First().FullName;
    }
}
