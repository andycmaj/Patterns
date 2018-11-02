#tool coveralls.io
#tool OpenCover
#addin Cake.Coveralls

#tool "nuget:?package=ReportGenerator&version=4.0.2"

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
// Unit Tests
var CSharpCoverageThreshold = Argument("coveragePercentThreshold", 5);
var CSharpCoverageExcludePatterns = new List<string>();

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
    Information($"Branch: {EnvironmentVariable("TRAVIS_BRANCH")}");
    Information($"Tag: {EnvironmentVariable("TRAVIS_TAG")}");
    Information($"Build configuration: {Configuration}");

        CSharpCoverageThreshold = 0;
    // CSharpCoverageExcludePatterns.Add("**/*.Designer.cs");
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

Task("DotNetTestWithCodeCoverage")
    .IsDependentOn("Build")
    .Does(() =>
{
    RunCoverlet(
        Configuration,
        CSharpCoverageThreshold,
        CSharpCoverageExcludePatterns.ToArray()
    );
});

public void RunCoverlet(
    string configuration,
    int coverageThreshold = 0,
    params string[] excludePatterns
)
{
    // TODO: https://github.com/Romanx/Cake.Coverlet

    if (excludePatterns.Any())
    {
        msBuildSettings.WithProperty("Exclude", $"\"{string.Join(",", excludePatterns)}\"");
    }

    var testSettings = new DotNetCoreTestSettings
    {
        ArgumentCustomization = args => {
            args.AppendMSBuildSettings(msBuildSettings, Context.Environment);
            return args;
        },
        Configuration = configuration,
        NoBuild = true,
    };

    var argBuilder = new ProcessArgumentBuilder();
    var hasCoverage = false;
    var projectFiles = GetFiles("./test/**/*.csproj");
    foreach (var projectFile in projectFiles)
    {
        var projectDir = projectFile.GetDirectory();
        Information($"Using {projectDir}, {projectFile}...");

        var msBuildSettings = new DotNetCoreMSBuildSettings()
            .WithProperty("CollectCoverage", "true")
            .WithProperty("Threshold", coverageThreshold.ToString())
            .WithProperty("ThresholdType", "line")
            .WithProperty("CoverletOutputFormat", "opencover");
            // .WithProperty("MergeWith", "");

        DotNetCoreTest(projectFile.FullPath, testSettings);

        hasCoverage = true;

        var coverageFile = File($"{projectDir}/coverage.opencover.xml");
        argBuilder.AppendSwitchQuoted("-reports", ":", coverageFile);
    }
    argBuilder.AppendSwitchQuoted("-targetdir", ":", "./.artifacts");

    if (hasCoverage)
    {
        DotNetCoreExecute("./.tools/ReportGenerator.4.0.0-rc3/tools/netcoreapp2.0/ReportGenerator.dll", argBuilder);
    }
    else
    {
        Information("no test coverage found");
    }
}

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
    .IsDependentOn("DotNetTestWithCodeCoverage");

Task("Publish")
    .IsDependentOn("Pack")
    .IsDependentOn("Push");

Task("Coverage")
    .IsDependentOn("UploadCodeCoverage");

RunTarget(DefaultTarget);
