using NugetBuildTargetsIntegrationTesting;
using NugetRepoReadme.MSBuild;
using NugetRepoReadme.RemoveReplace.Settings;

namespace EndToEndTests
{
    internal sealed class NugetRepoReadme_Tests
    {
        private const string DefaultPackageReadmeFileElementContents = "package-readme.md";
        private readonly NugetBuildTargetsTestSetup _nugetBuildTargetsTestSetup = new();

        private sealed record RepoReadme(string Readme, string RelativePath = "readme.md", bool AddProjectElement = true, bool AddReadme = true);

        private sealed record GeneratedReadme(
            string Expected,
            string PackageReadMeFileElementContents = DefaultPackageReadmeFileElementContents,
            string ZipEntryName = "package-readme.md",
            string ExpectedOutputPath = "obj\\Release\\net461\\ReadmeRewrite\\package-readme.md")
        {
            public static GeneratedReadme Simple(string expected) => new(expected);

            public static GeneratedReadme PackagePath(string expected, string packageReadMeFileElementContents)
                => new(expected, packageReadMeFileElementContents, packageReadMeFileElementContents.Replace('\\', '/'));

            public static GeneratedReadme OutputPath(string expected, string expectedOutputPath)
                => new(expected, ExpectedOutputPath: expectedOutputPath);
        }

        private sealed record ProjectFileAdditional(string Properties, string RemoveReplaceItems, string Targets)
        {
            public static ProjectFileAdditional None { get; } = new ProjectFileAdditional(string.Empty, string.Empty, string.Empty);

            public static ProjectFileAdditional PropertiesOnly(string properties) => new(properties, string.Empty, string.Empty);

            public static ProjectFileAdditional RemoveReplaceItemsOnly(string removeReplaceItems) => new(string.Empty, removeReplaceItems, string.Empty);
        }

        [OneTimeTearDown]
        public void TearDown() => _nugetBuildTargetsTestSetup.TearDown();

        [Test]
        public void Should_Have_Correct_ReadMe_In_Generated_NuPkg()
        {
            // relative to repo root
            const string relativeReadme = @"
Before
![image](/images/image.png)
After
";

            const string expectedNuGetReadme = @"
Before
![image](https://raw.githubusercontent.com/tonyhallett/arepo/master/images/image.png)
After
";

            Test(
                new RepoReadme(relativeReadme, "readmedir/readme.md"),
                GeneratedReadme.Simple(expectedNuGetReadme),
                null,
                addRelativeFile => addRelativeFile("images/image.png", string.Empty));
        }

        [Test]
        public void Should_Have_Correct_Replaced_To_End_Inline_Readme_In_Generated_NuPkg()
        {
            const string replace = "# Replace";
            const string replacement = "Nuget only";

            string removeReplaceItems = ReadmeRemoveReplaceItemString.Create(
                "1",
                [
                    ReadmeRemoveReplaceItemString.StartElement(replace),
                    ReadmeRemoveReplaceItemString.CommentOrRegexElement(CommentOrRegex.Regex),
                    ReadmeRemoveReplaceItemString.ReplacementTextElement(replacement)
                ]);

            string repoReadme = @$"
Before
{replace}
This will be replaced
";

            string expectedNuGetReadme = @$"
Before
{replacement}";

            Test(new RepoReadme(repoReadme), GeneratedReadme.Simple(expectedNuGetReadme), ProjectFileAdditional.RemoveReplaceItemsOnly(removeReplaceItems));

        }

        [Test]
        public void Should_Have_Correct_Replaced_To_End_From_File_Readme_In_Generated_NuPkg()
        {
            const string replace = "# Replace";
            const string replacement = "Nuget only file replace";

            string removeReplaceItems = ReadmeRemoveReplaceItemString.Create(
                "replace.txt",
                [
                    ReadmeRemoveReplaceItemString.StartElement(replace),
                    ReadmeRemoveReplaceItemString.CommentOrRegexElement(CommentOrRegex.Regex),
                    ReadmeRemoveReplaceItemString.ReplacementTextElement(replacement)
                ]);
            string relativeReadme = @$"
Before
{replace}
This will be replaced
";

            string expectedNuGetReadme = @$"
Before
{replacement}";

            Test(new RepoReadme(relativeReadme), GeneratedReadme.Simple(expectedNuGetReadme), ProjectFileAdditional.RemoveReplaceItemsOnly(removeReplaceItems), addRelativeFile => addRelativeFile("relative.text", replacement));
        }

        [Test]
        public void Should_Regex_Replace_With_Start_End_Escape_Chars()
        {
            string removeReplaceItems = ReadmeRemoveReplaceItemString.Create(
                "1",
                [
                    ReadmeRemoveReplaceItemString.StartElement("&lt;div>"),
                    ReadmeRemoveReplaceItemString.EndElement("&lt;/div>"),
                    ReadmeRemoveReplaceItemString.CommentOrRegexElement(CommentOrRegex.Regex),
                    ReadmeRemoveReplaceItemString.ReplacementTextElement("Replacement")
                ]);

            string repoReadme = @"
Before
<div>
This will be replaced
</div>
After";

            string expectedNuGetReadme = @"
Before
Replacement
After";

            Test(new RepoReadme(repoReadme), GeneratedReadme.Simple(expectedNuGetReadme), ProjectFileAdditional.RemoveReplaceItemsOnly(removeReplaceItems));

        }

        [Test]
        public void Should_Have_RepositoryCommit_When_PublishRepositoryUrl_GitHub() => PublishRepositoryUrlTest(
                "https://github.com/owner/repo.git",
                (repoRootRelativeImageUrl, commitId) => $"https://raw.githubusercontent.com/owner/repo/{commitId}{repoRootRelativeImageUrl}",
                (repoRootRelativeImageUrl, commitId) => $"https://github.com/owner/repo/blob/{commitId}{repoRootRelativeImageUrl}");

        [Test]
        public void Should_Have_RepositoryCommit_When_PublishRepositoryUrl_GitLab() => PublishRepositoryUrlTest(
                "https://gitlab.com/user/repo.git",
                (repoRootRelativeImageUrl, commitId) => $"https://gitlab.com/user/repo/-/raw/{commitId}{repoRootRelativeImageUrl}",
                (repoRootRelativeImageUrl, commitId) => $"https://gitlab.com/user/repo/-/blob/{commitId}{repoRootRelativeImageUrl}");

        private void PublishRepositoryUrlTest(
            string remoteUrl,
            Func<string, string, string> getExpectedAbsoluteImageUrl,
            Func<string, string, string> getExpectedAbsoluteLinkUrl)
        {
            const string additionalProperties = "<PublishRepositoryUrl>True</PublishRepositoryUrl>";

            const string commitId = "f5eb304528a94c667be2ab0f921b3995746c7ce8";
            string gitConfig = $"""
[core]
	repositoryformatversion = 0
[remote "origin"]
	url = {remoteUrl}

""";
            const string headBranchPath = "refs/heads/myBranch";
            string headContent = $"ref: {headBranchPath}";

            // relative to repo root
            const string relativeImageUrl = "/images/image.png";
            const string relativeLinkUrl = "/some/page.html";
            string repoReadme = $@"
![image]({relativeImageUrl})

[link]({relativeLinkUrl})";

            string expectedNuGetReadme = $@"
![image]({getExpectedAbsoluteImageUrl(relativeImageUrl, commitId)})

[link]({getExpectedAbsoluteLinkUrl(relativeLinkUrl, commitId)})";
            Test(
                new RepoReadme(repoReadme),
                GeneratedReadme.Simple(expectedNuGetReadme),
                projectFileAdditional: ProjectFileAdditional.PropertiesOnly(additionalProperties),
                addRelativeFileCallback: addRelativeFile =>
                {
                    addRelativeFile("images/image.png", string.Empty);
                    addRelativeFile("some/page.html", string.Empty);

                    addRelativeFile(".git/config", gitConfig);
                    addRelativeFile(".git/HEAD", headContent);
                    addRelativeFile($".git/{headBranchPath}", commitId);
                },
                addRepositoryUrl: false);
        }

        [Test]
        public void Should_Permit_Nested_Package_Path()
        {
            var repoReadme = new RepoReadme("untouched");
            var generatedReadme = GeneratedReadme.PackagePath("untouched", "docs\\package-readme.md");
            Test(repoReadme, generatedReadme);
        }

        [Test]
        public void Should_Generate_To_ReadmeRewrite_In_Obj() => Different_Output_Paths_Test(
            null,
            Path.Combine("obj", "Release", "net461", "ReadmeRewrite", DefaultPackageReadmeFileElementContents));

        [Test]
        public void Should_Generate_To_MSBuild_GeneratedReadmeDirectory_When_Absolute()
        {
            string tmpOutputDirectoryPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            try
            {
                Different_Output_Paths_Test(tmpOutputDirectoryPath, Path.Combine(tmpOutputDirectoryPath, DefaultPackageReadmeFileElementContents));
            }
            finally
            {
                if (File.Exists(tmpOutputDirectoryPath))
                {
                    File.Delete(tmpOutputDirectoryPath);
                }
            }
        }

        [Test]
        public void Should_Generate_Relative_To_Project_Directory_When_GeneratedReadmeDirectory_Is_Relative()
        {
            const string relativeProjectDir = "projectsubdir";
            Different_Output_Paths_Test(relativeProjectDir, Path.Combine(relativeProjectDir, DefaultPackageReadmeFileElementContents));
        }

        [Test]
        public void Should_Not_Error_On_Html_When_ErrorOnHtml_Not_Set()
        {
            var repoReadme = new RepoReadme("<div>Some html</div>");
            var generatedReadme = GeneratedReadme.Simple("<div>Some html</div>");
            Test(repoReadme, generatedReadme);
        }

        [Test]
        public void Should_Error_On_Html_When_ErrorOnHtml_Is_True()
        {
            var repoReadme = new RepoReadme("<div>Some html</div>");
            var generatedReadme = GeneratedReadme.Simple("<div>Some html</div>");
            bool hasThrown = false;
            try
            {
                Test(repoReadme, generatedReadme, ProjectFileAdditional.PropertiesOnly("<ErrorOnHtml>true</ErrorOnHtml>"));
            }
            catch (DotNetCommandException ex)
            {
                hasThrown = true;
                Assert.Multiple(() =>
                {
                    Assert.That(ex.Command, Is.EqualTo("build"));
                    Assert.That(ex.ToString(), Contains.Substring("Readme has unsupported HTML"));
                });
            }
            finally
            {
                Assert.That(hasThrown, Is.True);
            }
        }

        [Test]
        public void Should_Be_Able_To_Replace_Words()
        {
            var repoReadme = new RepoReadme("The word foo will be replaced.");
            var generatedReadme = GeneratedReadme.Simple("bar is a replacement.");
            const string removeReplaceFileName = "removereplacewordsfile.txt";
            const string removeReplaceFileContents =
@"Removals
---
The word 

Replacements
---
foo
bar
will be replaced
is a replacement
";
            var projectFileAdditional = ProjectFileAdditional.RemoveReplaceItemsOnly(
$@"<{MsBuildPropertyItemNames.ReadmeRemoveReplaceWordsItem} Include=""{removeReplaceFileName}""/>");
            Test(
                repoReadme,
                generatedReadme,
                projectFileAdditional,
                addRelativeFile => addRelativeFile(removeReplaceFileName, removeReplaceFileContents));
        }

        [Test]
        public void Should_Be_Able_To_Replace_Words_Regex()
        {
            var repoReadme = new RepoReadme("will _remove words starting with rem on word boundary");
            var generatedReadme = GeneratedReadme.Simple("will _remove words starting with  on word boundary");
            const string removeReplaceFileName = "removereplacewordsfile.txt";
            const string removeReplaceFileContents =
@"Removals
---
\brem[a-zA-Z]*\b";
            var projectFileAdditional = ProjectFileAdditional.RemoveReplaceItemsOnly(
$@"<{MsBuildPropertyItemNames.ReadmeRemoveReplaceWordsItem} Include=""{removeReplaceFileName}""/>");
            Test(
                repoReadme,
                generatedReadme,
                projectFileAdditional,
                addRelativeFile => addRelativeFile(removeReplaceFileName, removeReplaceFileContents));
        }

        [Test]
        public void Should_Be_Transformable_With_TransformNugetReadme_Target_And_NugetReadmeContent_Property()
        {
            var repoReadme = new RepoReadme("uppercasethis");
            var generatedReadme = GeneratedReadme.Simple("uppercasethis");
            const string target = """
<Target Name="TransformNugetReadme">
    <PropertyGroup>
        <NugetReadmeContent>$(NugetReadmeContent.ToUpper())</NugetReadmeContent>
    </PropertyGroup>
</Target>
""";
            var projectFileAdditional = new ProjectFileAdditional(string.Empty, string.Empty, target);
            Test(repoReadme, generatedReadme/*, projectFileAdditional*/);
        }

        private void Different_Output_Paths_Test(string? generatedReadmeDirectory, string expectedOutputPath)
        {
            var repoReadme = new RepoReadme("untouched");
            var generatedReadme = GeneratedReadme.OutputPath("untouched", expectedOutputPath);
            ProjectFileAdditional? projectFileAdditional = generatedReadmeDirectory == null ? null : ProjectFileAdditional.PropertiesOnly($"<GeneratedReadmeDirectory>{generatedReadmeDirectory}</GeneratedReadmeDirectory>");
            Test(repoReadme, generatedReadme, projectFileAdditional);
        }

        private void Test(
            RepoReadme repoReadme,
            GeneratedReadme generatedReadme,
            ProjectFileAdditional? projectFileAdditional = null,
            Action<Action<string, string>>? addRelativeFileCallback = null,
            bool addRepositoryUrl = true)
        {
            projectFileAdditional ??= ProjectFileAdditional.None;
            string baseReadmeElementOrEmptyString = repoReadme.AddProjectElement ? $"<BaseReadme>{repoReadme.RelativePath}</BaseReadme>" : string.Empty;
            string repositoryUrlElementOrEmptyString = addRepositoryUrl ? "<RepositoryUrl>https://github.com/tonyhallett/arepo.git</RepositoryUrl>" : string.Empty;
            DirectoryInfo? projectDirectory = null;
            string projectWithReadMe = $"""
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <TargetFramework>net461</TargetFramework>
        <Authors>TonyHUK</Authors>
        {repositoryUrlElementOrEmptyString}
        <PackageReadmeFile>{generatedReadme.PackageReadMeFileElementContents}</PackageReadmeFile>
        <PackageProjectUrl>https://github.com/tonyhallett/arepo</PackageProjectUrl>
        <GeneratePackageOnBuild>True</GeneratePackageOnBuild>
        {baseReadmeElementOrEmptyString}
        <IsPackable>True</IsPackable>
{projectFileAdditional.Properties}
     </PropertyGroup>
     <ItemGroup>
{projectFileAdditional.RemoveReplaceItems}
     </ItemGroup>
{projectFileAdditional.Targets}
</Project>
""";

            string nuPkgPath = NupkgProvider.GetNuPkgPath();
            _ = _nugetBuildTargetsTestSetup.Setup(
                projectWithReadMe,
                nuPkgPath,
                (projectPath) =>
                {
                    projectDirectory = new DirectoryInfo(Path.GetDirectoryName(projectPath)!);
                    void CreateRelativeFile(string relativeFile, string contents) => FileHelper.WriteAllTextEnsureDirectory(Path.Combine(projectDirectory.FullName, relativeFile), contents);
                    if (repoReadme.AddReadme)
                    {
                        CreateRelativeFile(repoReadme.RelativePath, repoReadme.Readme);
                    }

                    addRelativeFileCallback?.Invoke(CreateRelativeFile);
                });

            if (projectDirectory == null)
            {
                throw new Exception("Project directory not set");
            }

            string dependentNuGetReadMe = NupkgReadmeReader.Read(projectDirectory, generatedReadme.ZipEntryName);

            Assert.That(dependentNuGetReadMe, Is.EqualTo(generatedReadme.Expected));

            string expectedOutputPath = Path.IsPathRooted(generatedReadme.ExpectedOutputPath)
                ? generatedReadme.ExpectedOutputPath
                : Path.Combine(projectDirectory.FullName, generatedReadme.ExpectedOutputPath);
            Assert.That(File.Exists(expectedOutputPath), Is.True, $"Expected generated path {expectedOutputPath} to exist");
        }
    }
}
