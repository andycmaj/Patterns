//////////////////////////////////////////////////////////////////////
// IMPORTS AND REFERENCES
//////////////////////////////////////////////////////////////////////
#r "./.tools/ImsHealth.Patterns/lib/net451/ImsHealth.Patterns.dll"
#r "./.tools/Newtonsoft.Json/lib/net45/Newtonsoft.Json.dll"

// Uncomment absolute path and comment out relative path to use a local
// build of ImsHealth.Automation
#r "./.tools/ImsHealth.Automation/lib/net451/ImsHealth.Automation.dll"
//#r "./.artifacts/Debug/net451/ImsHealth.Automation.dll"
//#r "/Workspaces/appature/git/ImsHealth.Automation/.artifacts/Debug/net451/ImsHealth.Automation.dll"

#addin "Cake.FileHelpers"

using Cake.Core.Tooling;

//////////////////////////////////////////////////////////////////////
// DEFAULT ARGUMENTS
//////////////////////////////////////////////////////////////////////

var OutputPath = Argument("outputPath", GetDefaultOutputPath());
var Configuration = Argument("configuration", "Debug");
var RestorePackagesHere = Argument("restorePackagesHere", false);
var TestResultsPath =
    File(Argument("testResultsPath", Directory(OutputPath) + File("test_results.xml")))
        .Path
        .MakeAbsolute(Context.Environment);
var TestParallelism = Argument("testParallelism", ParallelismOption.None);
var SolutionFile = GetFiles("./*.sln").SingleOrDefault();
var GlobalJsonFile = File("./global.json");
var IsProjectJson = FileExists(GlobalJsonFile) && FileReadText(GlobalJsonFile).Contains("preview2");

//////////////////////////////////////////////////////////////////////
// SETTINGS
//////////////////////////////////////////////////////////////////////

var HOME_DIR = IsRunningOnWindows()
    ? EnvironmentVariable("USERPROFILE")
    : EnvironmentVariable("HOME");

var NUGET_PUSH_SETTINGS =
    new NuGetPushSettings
    {
        ToolPath = File("./.tools/nuget.exe")
    };

var DOTNET_RESTORE_SETTINGS =
    new DotNetCoreRestoreSettings
    {
        DisableParallel = false
    };
if (RestorePackagesHere)
{
    DOTNET_RESTORE_SETTINGS.PackagesDirectory = "./.nuget";
}

var DOTNET_PACK_SETTINGS = new DotNetCorePackSettings();
var isReleaseBuild = Configuration == "Release";
if (isReleaseBuild)
{
    Information($"Release Build");
}
else
{
    var buildNumber = $"t{DateTime.UtcNow.ToString("yyMMddHHmmss")}";

    DOTNET_PACK_SETTINGS.VersionSuffix = buildNumber;
    Information($"Prerelease Build Number: {buildNumber}");
}

var DOTNET_TEST_SETTINGS =
    new DotNetCoreTestSettings
    {
        Configuration = Configuration,
        ArgumentCustomization = args => args
            .Append("-verbose")
            .AppendSwitchQuoted("-xml", TestResultsPath.FullPath)
    };

//////////////////////////////////////////////////////////////////////
// HELPERS
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

// Backwards compatibility
public void ForEachProject(string globPattern, Action<DirectoryPath> projectAction)
{
    var projectFiles = GetFiles($"{globPattern}/project.json").Union(GetFiles($"{globPattern}/*.csproj"));
    foreach (var projectFile in projectFiles)
    {
        var projectDir = projectFile.GetDirectory();
        Information($"Using {projectDir}, {projectFile}...");
        projectAction(projectDir);
    }
}

public void ForEachDockerProject(string globPattern, Action<DirectoryPath> dockerProjectAction)
{
    var dockerfiles = GetFiles($"{globPattern}/Dockerfile");
    foreach (var dockerfilePath in dockerfiles.Select(file => file.GetDirectory()))
    {
        Information($"Using {dockerfilePath}...");
        dockerProjectAction(dockerfilePath);
    }
}

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(OutputPath);
    Func<IFileSystemInfo, bool> exclude_node_modules =
        fileSystemInfo => !fileSystemInfo.Path.FullPath.Contains("node_modules", StringComparison.OrdinalIgnoreCase);
    CleanDirectories("**/bin", exclude_node_modules);
    CleanDirectories("**/obj", exclude_node_modules);
});

Task("Restore")
    .Does(() =>
{
    if (!IsProjectJson)
    {
        Information($"Using solution file to restore packages: {SolutionFile}");
        DotNetCoreRestore(SolutionFile.FullPath, DOTNET_RESTORE_SETTINGS);
    }
    else
    {
        Information("Legacy dotnet tooling found. Restoring from root of repo");
        DotNetCoreRestore(DOTNET_RESTORE_SETTINGS);
    }
});

Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
{
    if (!IsProjectJson)
    {
        Information($"Using solution file to build projects: {SolutionFile}");
        DotNetCoreBuild(SolutionFile.FullPath);
    }
    else
    {
        Information($"Using solution file to build projects: {SolutionFile}");
        ForEachProject("./**", (projectDir, projectFile) => {
            DotNetCoreBuild(projectDir.FullPath);
        });
    }
});

// HACK for dotnet test preview2.
// Remove test-mono and only use DotNetCoreTest task when mono xunit runner bug is fixed
Task("Test-Windows")
    .WithCriteria(() => IsRunningOnWindows())
    .Does(() =>
{
    var testAssemblies = GetFiles("./test/**/bin/*/*/*/*Tests.dll");
    if (!testAssemblies.Any()) {
       throw new Exception("Found no test assemblies to run. Failing.");
    }
    foreach (var assembly in testAssemblies)
    {
        Information("Running tests from {0}", assembly);
        var testSuccess =
            StartProcess(
                $"{assembly.GetDirectory()}/dotnet-test-xunit.exe",
                new ProcessSettings {
                    WorkingDirectory = assembly.GetDirectory(),
                    Arguments = $"{assembly.FullPath} -verbose -xml {TestResultsPath}"
                }
            );

        if (testSuccess != 0)
        {
            throw new Exception("Tests failed in project: " + assembly);
        }
    }

});

// HACK for dotnet test preview2.
// Remove test-mono and only use DotNetCoreTest task when mono xunit runner bug is fixed
Task("Test-Mono")
    .WithCriteria(() => IsRunningOnUnix())
    .Does(() =>
{
    var testAssemblies = GetFiles("./test/**/bin/*/*/*/*Tests.dll");
    if (!testAssemblies.Any()) {
       throw new Exception("Found no test assemblies to run. Failing.");
    }
    foreach (var assembly in testAssemblies)
    {
        Information("Running tests from {0}", assembly);
        var testSuccess =
            StartProcess(
                "mono",
                new ProcessSettings {
                    WorkingDirectory = assembly.GetDirectory(),
                    Arguments = $"dotnet-test-xunit.exe {assembly.FullPath} -verbose -xml {TestResultsPath}"
                }
            );

        if (testSuccess != 0)
        {
            throw new Exception("Tests failed in project: " + assembly);
        }
    }
});

Task("TestLegacy")
    // HACK: Don't need to depend on Build once the dotnet test bug is fixed in dotnet cli
    .IsDependentOn("Build")
    .IsDependentOn("Test-Mono")
    .IsDependentOn("Test-Windows");

Task("Test")
    .IsDependentOn("Restore")
    .Does(() =>
{
    ForEachProject("./test/*.Tests", (projectDir, projectFile) => {
        DotNetCoreTest(projectFile.FullPath);
    });
});

Task("Pack")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .Does(() =>
{
    ForEachProject("./**", (projectDir, projectFile) => {
        DotNetCorePack(projectDir.FullPath, DOTNET_PACK_SETTINGS);
    });

    // HACK: Pack in place because otherwise package is generated with bad dir structure
    MoveFiles(GetFiles("./src/*/bin/**/*.nupkg"), OutputPath);
});

Task("PublishLocal")
    .IsDependentOn("Pack")
    .Does(() =>
{
    CopyFiles($"{OutputPath}*.nupkg", $"{HOME_DIR}/.nuget");
});

Task("Publish")
    .IsDependentOn("Pack")
    .Does(() =>
{
    foreach (var packageFile in GetFiles(OutputPath + "**/*.nupkg"))
    {
        Information($"Publishing: {packageFile.FullPath}");

        NuGetPush(packageFile, NUGET_PUSH_SETTINGS);
    }
});
