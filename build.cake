#tool coveralls.io
#tool OpenCover
#addin Cake.Coveralls

//////////////////////////////////////////////////////////////////////
// ARGUMENT DEFAULTS
//////////////////////////////////////////////////////////////////////

var DefaultTarget = Argument("target", "Default");
var OutputPath = Argument("outputPath", ".artifacts");
var TestResultsPath =
    Directory(Argument("testResultsPath", OutputPath))
        .Path
        .MakeAbsolute(Context.Environment);
var XUnitArguments = Argument("xUnitArgs", "-parallel none -verbose -xml {0}");

//////////////////////////////////////////////////////////////////////
// Helpers
//////////////////////////////////////////////////////////////////////

public void ForEachProject(string globPattern, Action<DirectoryPath, FilePath> projectAction)
{
    var projectFiles = GetFiles($"{globPattern}/project.json").Union(GetFiles($"{globPattern}/*.csproj"));
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
    DotNetCoreBuild(".");
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
            Directory(OutputPath) + File("coverage.xml"),
            new OpenCoverSettings {
                // Must use projectDir as working dir or else dotnet-xunit doesn't work when driven by OpenCover cli
                WorkingDirectory = projectDir,
                Register = "user",
                OldStyle = true
            }
                .WithFilter("+[Patterns]*")
                .WithFilter("+[Patterns.SimpleInjector]*")
                .WithFilter("-[Patterns.Tests]*")
                .WithFilter("-[xunit*]*")
        )
    )
);

Task("UploadCodeCoverage")
    .WithCriteria(() => IsRunningOnWindows())
    .IsDependentOn("MeasureCodeCoverage")
    .Does(() => CoverallsIo(
        Directory(OutputPath) + File("coverage.xml"),
        new CoverallsIoSettings
        {
            RepoToken = EnvironmentVariable("COVERALLS_REPO_TOKEN")
        }
    )
);

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Test");

Task("Coverage")
    .IsDependentOn("MeasureCodeCoverage")
    .IsDependentOn("UploadCodeCoverage");

RunTarget(DefaultTarget);
