#tool coveralls.io
#tool OpenCover
#addin Cake.Coveralls

//////////////////////////////////////////////////////////////////////
// ARGUMENT DEFAULTS
//////////////////////////////////////////////////////////////////////

var DefaultTarget = Argument("target", "Default");
var Configuration = Argument("configuration", "Debug");
var OutputPath = Argument("outputPath", ".artifacts");
var TestResultsPath =
    Directory(Argument("testResultsPath", OutputPath))
        .Path
        .MakeAbsolute(Context.Environment);
var XUnitArguments = Argument("xUnitArgs", "-parallel none -verbose -xml {0}");

// TODO: update when we have multiple test projects
var CoverageResultsPath = Directory(OutputPath) + File("coverage.xml");

//////////////////////////////////////////////////////////////////////
// Helpers
//////////////////////////////////////////////////////////////////////

public void ForEachProject(string globPattern, Action<DirectoryPath, FilePath> projectAction)
{
    var projectFiles = GetFiles($"{globPattern}/*.csproj");
    foreach (var projectFile in projectFiles)
    {
        var projectDir = projectFile.GetDirectory();
        Information($"Using {projectDir}, {projectFile}...");
        projectAction(projectDir, projectFile);
    }
}

// Need to use Func syntax to access static Argument vars defined above
Func<FilePath, FilePath> MakeTestResultFile = (projectFile) =>
{
    var projectName = projectFile.GetFilenameWithoutExtension();
    return TestResultsPath.GetFilePath(File($"{projectName}.Results.xml"));
};

//////////////////////////////////////////////////////////////////////
// Tasks
//////////////////////////////////////////////////////////////////////

Setup(context =>
{
    Information($"Build configuration: {Configuration}");
});

Task("EnsureOutputPathExists")
    .Does(() => EnsureDirectoryExists(OutputPath));

Task("Restore")
    .Does(() =>
{
    DotNetCoreRestore();
});

Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
{
    DotNetCoreBuild(".", new DotNetCoreBuildSettings { Configuration = Configuration });
});

Task("Test")
    .IsDependentOn("EnsureOutputPathExists")
    .IsDependentOn("Restore")
    .Does(() => ForEachProject("./test/*.Tests", (projectDir, projectFile) =>
        DotNetCoreTool(
            projectFile,
            "xunit",
            string.Format(XUnitArguments, MakeTestResultFile(projectFile))
        )
    )
);

// UberHack: must use debugType:full on Windows only when running code-coverage.
// TODO: unhack when https://github.com/OpenCover/opencover/issues/601 is resolved
Task("ShimProjectDebugTypesForOpenCover")
    .WithCriteria(() => IsRunningOnWindows())
    .Does(() => ForEachProject("./src/*", (projectDir, projectFile) =>
        XmlPoke(
            projectFile,
            "/Project/PropertyGroup/DebugType",
            "full"
        )
    )
);

Task("MeasureCodeCoverage")
    .WithCriteria(() => IsRunningOnWindows())
    .IsDependentOn("EnsureOutputPathExists")
    .IsDependentOn("ShimProjectDebugTypesForOpenCover")
    .IsDependentOn("Restore")
    .Does(() => ForEachProject("./test/*.Tests", (projectDir, projectFile) =>
        OpenCover(
            context => context.DotNetCoreTool(
                projectFile,
                "xunit"
            ),
            CoverageResultsPath,
            new OpenCoverSettings {
                // Must use projectDir as working dir or else dotnet-xunit doesn't work when driven by OpenCover cli
                WorkingDirectory = projectDir,
                Register = "user",
                OldStyle = true
            }
                .WithFilter("+[AndyC.Patterns]*")
                .WithFilter("+[AndyC.Patterns.SimpleInjector]*")
                .WithFilter("-[AndyC.Patterns.Tests]*")
                .WithFilter("-[xunit*]*")
        )
    )
);

Task("UploadCodeCoverage")
    .WithCriteria(() => IsRunningOnWindows() && FileExists(CoverageResultsPath))
    .IsDependentOn("MeasureCodeCoverage")
    .Does(() => CoverallsIo(
        CoverageResultsPath,
        new CoverallsIoSettings
        {
            RepoToken = EnvironmentVariable("COVERALLS_REPO_TOKEN")
        }
    )
);

Task("Pack")
    .Does(() => ForEachProject("./src/*", (projectDir, projectFile) => {
        var settings = new DotNetCorePackSettings
        {
            Configuration = Configuration,
            OutputDirectory = OutputPath
        };

        var isReleaseBuild = Configuration == "Release";
        if (isReleaseBuild)
        {
            Information($"Release Build");
        }
        else
        {
            var buildNumber = $"t{DateTime.UtcNow.ToString("yyMMddHHmmss")}";

            settings.VersionSuffix = buildNumber;
            Information($"Prerelease Build Number: {buildNumber}");
        }

        DotNetCorePack(projectDir.FullPath, settings);
    })
);

Task("Push")
    .Does(() =>
{
    DotNetCoreNuGetPush(
        OutputPath + "/*.nupkg",
        new DotNetCoreNuGetPushSettings
        {
            Source = "https://www.nuget.org/api/v2/package",
            ApiKey = EnvironmentVariable("NUGET_API_KEY")
        }
    );
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Test");

Task("Publish")
    .IsDependentOn("Pack")
    .IsDependentOn("Push");

Task("Coverage")
    .IsDependentOn("MeasureCodeCoverage")
    .IsDependentOn("UploadCodeCoverage");

RunTarget(DefaultTarget);
