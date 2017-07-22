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
    .Does(() =>
{
    ForEachProject("./test/*.Tests", (projectDir, projectFile) =>
        DotNetCoreTool(
            projectFile,
            "xunit",
            string.Format(XUnitArguments, MakeTestResultFile(projectFile))
        )
    );
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Test");

// Task("BuildAndPublish")
//     .IsDependentOn("Pack")
//     .IsDependentOn("Publish");

RunTarget(DefaultTarget);
