# Description

[MsBuild custom build targets](https://learn.microsoft.com/en-us/nuget/concepts/msbuild-props-and-targets) for NuGet packages that

First target - TransformNugetReadme

1. Reads the README file from the project.
2. Allows removal or replacement of parts of the README file.
3. Converts relative GitHub or GitLab URLs in the README file to raw URLs.
4. Has behaviour settings for handling HTML in the README file.
5. Outputs MSBuild property NugetReadmeContent

Second target - WriteNugetReadme

This target depends on TransformNugetReadme via the property WriteNugetReadmeDependsOn.
If you wish to transform the readme contents from TransformNugetReadme then add your target to WriteNugetReadmeDependsOn and use NugetReadmeContent.
```xml
<PropertyGroup>
  <WriteNugetReadmeDependsOn>TransformNugetReadme</WriteNugetReadmeDependsOn>
</PropertyGroup>
<Target Name="TransformNugetReadme">
    <PropertyGroup>
        <NugetReadmeContent>$(NugetReadmeContent.ToUpper())</NugetReadmeContent>
    </PropertyGroup>
</Target>

```

1. Writes NugetReadmeContent to file and packs the readme, so you [do not need to](https://learn.microsoft.com/en-us/nuget/reference/errors-and-warnings/nu5039)

[GitLab supported markup languages](https://docs.gitlab.com/user/project/repository/files/#supported-markup-languages)

[GitLab markdown](https://docs.gitlab.com/user/markdown/)

[Github about readmes](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-readmes#relative-links-and-image-paths-in-markdown-files)

[Github markdown](https://github.github.com/gfm/)

[Nuget package readme](https://learn.microsoft.com/en-us/nuget/nuget-org/package-readme-on-nuget-org)

Note that nuget will display html and markdown it does not understand literally.

# MSBuild Task Properties

| Property                            | Default   | Required                   | Nuget Property | Description                                                                   |
| ----------------------------------- | --------- | -------------------------- | -------------- | ----------------------------------------------------------------------------- |
| BaseReadme                          | README.md | No                         |                | The readme relative path to transform                                         |
| PackageReadmeFile                   |           | Yes.                       | Yes            | Used for the PackagePath, file name used for the output file                  |
| GeneratedReadmeDirectory            |           | No                         |                | Defaults to ReadmeRewrite in obj. If relative will be relative to the project |
|                                     |           |                            |                |                                                                               |
|                                     |           |                            |                | A GitHub or GitLab repository url ( .git ) - order of precedence              |
| ReadmeRepositoryUrl                 |           | Not if RepositoryUrl       |                |                                                                               |
| RepositoryUrl                       |           | Not if ReadmeRepositoryUrl | Yes            |                                                                               |
|                                     |           |                            |                |                                                                               |
|                                     |           |                            |                | The ref part of the generated absolute url in order of precedence             |
| RepositoryRef                       | master    | No.                        |                |                                                                               |
| RepositoryCommit                    | master    | No.                        | Yes            |                                                                               |
| RepositoryBranch                    | master    | No.                        | Yes            |                                                                               |
|                                     |           |                            |                |                                                                               |
| RemoveCommentIdentifiers            |           | No.                        |                | The format is - _startidentifier_;_endidentifier_ or _startidentifier_        |
|                                     |           |                            |                |                                                                               |
| ErrorOnHtml                         |           |                            |                | Set this to true to fail the build when BaseReadme has html                   |
| RemoveHtml                          |           |                            |                | Set this to true to remove html in BaseReadme                                 |
| ExtractDetailsContentWithoutSummary |           |                            |                | Set this to true for contents of Html Detail elements to be extracted         |

Of the ref MSBuild properties, RepositoryCommit is probably what you should be used.
Note that with SDK style projects the RepositoryUrl, RepositoryCommit and RepositoryBranch properties are automatically populated from the .git directory
if you set the MSBuild PublishRepositoryUrl property to True.

Non SDK style you can add the nuget package [Microsoft.Build.Tasks.Git](https://www.nuget.org/packages/Microsoft.Build.Tasks.Git).
This is included with [Microsoft.SourceLink.GitHub](https://www.nuget.org/packages/Microsoft.SourceLink.GitHub/) and [Microsoft.SourceLink.GitLab](https://www.nuget.org/packages/Microsoft.SourceLink.GitLab/).

# Removal / replacement

Removal or replacement of sections can be specified in two ways.
If you wish to mark the BaseReadme with comments to identify parts to remove then you can use RemoveCommentIdentifiers.

e.g

```xml
<RemoveCommentIdentifiers>remove-start;remove-end</RemoveCommentIdentifiers>
```

```md
This is visible

<!-- remove-start 1 -->

This is removed

<!-- remove-end 1 -->

This is also visible

<!-- remove-start 2 -->

This is removed

<!-- remove-end 2 -->

This too is visible
```

This is the regex used internally. For RemoveCommentIdentifiers exact is false.

```cs
    public static Regex CreateRegex(string commentIdentifier, bool exact)
    {
        string end = exact ? @"\s*" : @"\b[^>]*";
        string pattern = @"<!--\s*" + Regex.Escape(commentIdentifier) + end + "-->";
        return new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }
```

If you want to remove remaining then RemoveCommentIdentifiers only needs start.

```md
This is visible

<!-- remove-start -->

This is removed
```

If you want to use regexes instead of comments or you want to do replacements then you need to use the msbuild item ReadmeRemoveReplace.
For each section that you want to remove or replace you need to add an item.

The available metadata is :

| Metadata        | Required | Description                                                                                                          |
| --------------- | -------- | -------------------------------------------------------------------------------------------------------------------- |
| CommentOrRegex  | Yes      | Either 'Comment' or 'Regex'. If 'Comment' then the same regex will be used as per RemoveCommentIdentifiers but exact |
| Start           | Yes      | A comment identifier or regex for the start of a section to be removed or replaced.                                  |
| End             | No       | A comment identifier or regex for the end of a section. If null then removes or replaces to the end.                 |
| ReplacementText | No       | For supplying inline, not from a file.                                                                               |

You can supply replacement text in two ways. Either inline using the ReplacementText metadata or from a file using the Include attribute.
If there is no replacement text then the matching section is removed.

There is also a special marker that can be used in replacement text. If replacement text contains "{readme_marker}" that will be replaced with the absolute url to the repository BaseReadme.

e.g

```md
Common to repo readme and nuget readme

# GitHub or GitLab eyes only

this will be replaced
```

Inline replacement text ( note the Include attribute is required even if not using a file )

```xml
<ReadmeRemoveReplace Include="1">
  <CommentOrRegex>Regex</CommentOrRegex>
  <Start>GitHub or GitLab eyes only</Start>
  <ReplacementText>For full details [see]({readme_marker})</ReplacementText>>
</ReadmeRemoveReplace>
```

You can also remove "words" by proving a file with this format

```
Removals
---
regex1
regex2

Replacements
---
regex1
replacement1
```

```xml
<ReadmeRemoveReplaceWords Include="pathtofile"/>
```

# How the regex matching and removal / replacement is performed.

Regex matching is done a line by line basis.
First "words" are removed/replaced.
Then sections:
Start is against the full line. Anything before the match is kept.
Corresponding End matching will be looked for in two places
a) On the same line as start - matched to what is remaining after start match
Anything after the match is kept
B) On its own line - matched as per start
Anything after the match is kept

# Failing the build

The relative BaseReadme does not exist.

The BaseReadme after removal and replacement contains html and ErrorOnHtml.

The BaseReadme has absolute image urls that are not from [Allowed domains](https://learn.microsoft.com/en-us/nuget/nuget-org/package-readme-on-nuget-org#allowed-domains-for-images-and-badges)

The BaseReadme has relative links that do not exist in the project.

The RepositoryUrl or ReadmeRepositoryUrl property is not a valid GitHub or GitLab url.

The RemoveCommentIdentifiers property is in an invalid format.

ReadmeRemoveReplace required metadata is missing.

ReadmeRemoveReplace Start and End are the same.
