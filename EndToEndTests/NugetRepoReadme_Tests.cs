using NugetBuildTargetsIntegrationTesting;
using NugetRepoReadme.MSBuild;
using NugetRepoReadme.RemoveReplace.Settings;

namespace EndToEndTests
{
    internal sealed class NugetRepoReadme_Tests
    {
        private const string DefaultPackageReadmeFileElementContents = "package-readme.md";
        private readonly NugetBuildTargetsTestSetupBuilder _nugetBuildTargetsTestSetupBuilder = new();

        private sealed record RepoReadme(
            string Readme,
            string BaseReadmeRelativePath = "readme.md",
            string ProjectContainingDirectoryRelativePath = "readme.md",
            bool AddProjectElement = true,
            bool AddReadme = true);

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

        private sealed record ProjectFileAdditional(string Properties, string RemoveReplaceItems, string Targets, string TargetFrameworks)
        {
            public static ProjectFileAdditional None { get; } = new ProjectFileAdditional(string.Empty, string.Empty, string.Empty, string.Empty);

            public static ProjectFileAdditional PropertiesOnly(string properties) => new(properties, string.Empty, string.Empty, string.Empty);

            public static ProjectFileAdditional RemoveReplaceItemsOnly(string removeReplaceItems) => new(string.Empty, removeReplaceItems, string.Empty, string.Empty);
        }

        [OneTimeTearDown]
        public void TearDown() => _nugetBuildTargetsTestSetupBuilder.TearDown();

        [Test]
        public void Should_Have_Correct_ReadMe_In_Generated_NuPkg_Repo_Relative()
        {
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
                new RepoReadme(relativeReadme, "readmedir/readme.md", "readmedir/readme.md"),
                GeneratedReadme.Simple(expectedNuGetReadme),
                null,
                [(string.Empty, "images/image.png")]);
        }

        [Test]
        public void Should_Have_Correct_ReadMe_In_Generated_NuPkg_Readme_Relative()
        {
            /*
                project structure
                Root
                    csproj
                    readmedir
                        readme.md
                        assetsdir
                            relative.txt
            */
            var repoReadme = new RepoReadme("[relative](assetsdir/relative.txt)", "readmedir/readme.md", "readmedir/readme.md");
            var generatedReadme = GeneratedReadme.Simple("[relative](https://github.com/tonyhallett/arepo/blob/master/readmedir/assetsdir/relative.txt)");

            Test(
                repoReadme,
                generatedReadme,
                additionalFiles: [
                    (string.Empty, "readmedir/assetsdir/relative.txt"),
                ]);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Work_With_BaseReadme_Outside_Project_Directory(bool repoRelative)
        {
            string repoRelativeOrReadmeRelativePrefix = repoRelative ? "/" : string.Empty;
            var repoReadme = new RepoReadme($"[relative]({repoRelativeOrReadmeRelativePrefix}relative.txt)", "../readme.md", "readme.md");
            var generatedReadme = GeneratedReadme.Simple("[relative](https://github.com/tonyhallett/arepo/blob/master/relative.txt)");

            const string solutionStyleProject = "DependentProject/dependentProject.csproj";

            Test(
                repoReadme,
                generatedReadme,
                additionalFiles: [
                    (string.Empty, "relative.txt"),
                ],
                projectRelativePath: solutionStyleProject);
        }

        #region replacement
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

            Test(
                new RepoReadme(repoReadme),
                GeneratedReadme.Simple(expectedNuGetReadme),
                ProjectFileAdditional.RemoveReplaceItemsOnly(removeReplaceItems));

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

            Test(
                new RepoReadme(relativeReadme),
                GeneratedReadme.Simple(expectedNuGetReadme),
                ProjectFileAdditional.RemoveReplaceItemsOnly(removeReplaceItems),
                [("relative.text", replacement)]);
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

            const string repoReadme = @"
Before
<div>
This will be replaced
</div>
After";

            const string expectedNuGetReadme = @"
Before
Replacement
After";

            Test(
                new RepoReadme(repoReadme),
                GeneratedReadme.Simple(expectedNuGetReadme),
                ProjectFileAdditional.RemoveReplaceItemsOnly(removeReplaceItems));

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
                [(removeReplaceFileContents, removeReplaceFileName)]);
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
                [(removeReplaceFileContents, removeReplaceFileName)]);
        }

        #endregion

        #region PublishRepositoryUrl
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
            const string publishRepositoryUrlProperty = "<PublishRepositoryUrl>True</PublishRepositoryUrl>";

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
                projectFileAdditional: ProjectFileAdditional.PropertiesOnly(publishRepositoryUrlProperty),
                additionalFiles: [
                    (string.Empty, "images/image.png"),
                    (string.Empty, "some/page.html"),
                    (gitConfig, ".git/config"),
                    (headContent, ".git/HEAD"),
                    (commitId, $".git/{headBranchPath}")
                ],
                addRepositoryUrl: false,
                addGit: false);
        }
        #endregion

        [Test]
        public void Should_Permit_Nested_Package_Path()
        {
            var repoReadme = new RepoReadme("untouched");
            var generatedReadme = GeneratedReadme.PackagePath("untouched", "docs\\package-readme.md");
            Test(repoReadme, generatedReadme);
        }

        #region output paths
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

        private void Different_Output_Paths_Test(string? generatedReadmeDirectory, string expectedOutputPath)
        {
            var repoReadme = new RepoReadme("untouched");
            var generatedReadme = GeneratedReadme.OutputPath("untouched", expectedOutputPath);
            ProjectFileAdditional? projectFileAdditional = generatedReadmeDirectory == null ? null : ProjectFileAdditional.PropertiesOnly($"<GeneratedReadmeDirectory>{generatedReadmeDirectory}</GeneratedReadmeDirectory>");
            Test(repoReadme, generatedReadme, projectFileAdditional);
        }
        #endregion

        #region ErrorOnHtml
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

            Test(
                repoReadme,
                generatedReadme,
                ProjectFileAdditional.PropertiesOnly("<ErrorOnHtml>true</ErrorOnHtml>"),
                failureAssertion: processResult => Assert.That(processResult.Output, Contains.Substring("Readme has unsupported HTML")));
        }
        #endregion

        [Test]
        public void Should_Be_Transformable_With_TransformNugetReadme_Target_And_NugetReadmeContent_Property()
        {
            var repoReadme = new RepoReadme("uppercasethis");
            var generatedReadme = GeneratedReadme.Simple("UPPERCASETHIS");
            const string target = """
<Target Name="TransformNugetReadme">
    <PropertyGroup>
        <NugetReadmeContent>$(NugetReadmeContent.ToUpper())</NugetReadmeContent>
    </PropertyGroup>
</Target>
""";
            const string writeNugetReadmeDependsOnProperty = "<WriteNugetReadmeDependsOn>TransformNugetReadme</WriteNugetReadmeDependsOn>";
            var projectFileAdditional = new ProjectFileAdditional(writeNugetReadmeDependsOnProperty, string.Empty, target, string.Empty);
            Test(repoReadme, generatedReadme, projectFileAdditional);
        }

        [Test]
        public void Should_Work_With_Multi_Targeting()
        {
            var repoReadme = new RepoReadme("untouched");
            var generatedReadme = new GeneratedReadme("untouched", ExpectedOutputPath: Path.Combine("obj", "Release", "ReadmeRewrite", "package-readme.md"));
            var projectFileAdditional = new ProjectFileAdditional(string.Empty, string.Empty, string.Empty, "net472;net6.0");
            Test(repoReadme, generatedReadme, projectFileAdditional);
        }

        private void Test(
            RepoReadme repoReadme,
            GeneratedReadme generatedReadme,
            ProjectFileAdditional? projectFileAdditional = null,
            IEnumerable<(string Contents, string RelativePath)>? additionalFiles = null,
            bool addRepositoryUrl = true,
            bool addGit = true,
            string projectRelativePath = "dependentProject.csproj",
            Action<IBuildResult>? failureAssertion = null)
        {
            string projectWithReadMe = GetProjectWithReadme(projectFileAdditional, repoReadme, generatedReadme.PackageReadMeFileElementContents, addRepositoryUrl);
            IBuildResult buildResult = _nugetBuildTargetsTestSetupBuilder
                .CreateProject()
                .AddFiles(GetFiles(additionalFiles, repoReadme, addGit))
                .AddProject(projectWithReadMe, projectRelativePath)
                .AddNuPkg(NupkgProvider.GetNuPkgPath())
                .BuildWithDotNet(); // use (-c Release -bl) for debugging

            if (!ProcessFailure(buildResult, failureAssertion))
            {
                return;
            }

            AssertGenerated(buildResult, generatedReadme);
        }

        private static string GetProjectWithReadme(
            ProjectFileAdditional? projectFileAdditional,
            RepoReadme repoReadme,
            string packageReadMeFileElementContents,
            bool addRepositoryUrl)
        {
            projectFileAdditional ??= ProjectFileAdditional.None;
            string baseReadmeElementOrEmptyString = repoReadme.AddProjectElement ? $"<BaseReadme>{repoReadme.BaseReadmeRelativePath}</BaseReadme>" : string.Empty;
            string repositoryUrlElementOrEmptyString = addRepositoryUrl ? "<RepositoryUrl>https://github.com/tonyhallett/arepo.git</RepositoryUrl>" : string.Empty;
            string targetFrameworkOrFrameworksElement = string.IsNullOrEmpty(projectFileAdditional.TargetFrameworks)
                ? "<TargetFramework>net461</TargetFramework>"
                : $"<TargetFrameworks>{projectFileAdditional.TargetFrameworks}</TargetFrameworks>";
            return $"""
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
 {targetFrameworkOrFrameworksElement}
        <Authors>TonyHUK</Authors>
        {repositoryUrlElementOrEmptyString}
        <PackageReadmeFile>{packageReadMeFileElementContents}</PackageReadmeFile>
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
        }

        private static List<(string Contents, string RelativePath)> GetFiles(
            IEnumerable<(string Contents, string RelativePath)>? additionalFiles,
            RepoReadme repoReadme,
            bool addGit)
        {
            List<(string Contents, string RelativePath)> files = additionalFiles == null ? [] : [.. additionalFiles];
            if (repoReadme.AddReadme)
            {
                files.Add((repoReadme.Readme, repoReadme.ProjectContainingDirectoryRelativePath));
            }

            if (addGit)
            {
                files.Add(("ref: refs/heads/main", ".git/HEAD"));
            }

            return files;
        }

        private static void AssertGenerated(IBuildResult buildResult, GeneratedReadme generatedReadme)
        {
            string dependentNuGetReadMe = NupkgReadmeReader.Read(buildResult.ProjectDirectory, generatedReadme.ZipEntryName);

            Assert.That(dependentNuGetReadMe, Is.EqualTo(generatedReadme.Expected));

            string expectedOutputPath = Path.IsPathRooted(generatedReadme.ExpectedOutputPath)
                ? generatedReadme.ExpectedOutputPath
                : Path.Combine(buildResult.ProjectDirectory.FullName, generatedReadme.ExpectedOutputPath);
            Assert.That(File.Exists(expectedOutputPath), Is.True, $"Expected generated path {expectedOutputPath} to exist");
        }

        private static bool ProcessFailure(IBuildResult buildResult, Action<IBuildResult>? failureAssertion)
        {
            if (!buildResult.Passed)
            {
                if (failureAssertion != null)
                {
                    failureAssertion(buildResult);
                    return false;
                }

                Assert.Fail(buildResult.ErrorAndOutput);
            }

            if (failureAssertion != null)
            {
                Assert.Fail("Expected failure but no failure");
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
