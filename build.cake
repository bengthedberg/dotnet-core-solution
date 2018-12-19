#tool "nuget:?package=GitVersion.CommandLine"
#addin "nuget:?package=Cake.DependenciesAnalyser&version=2.0.0"

#load "./scripts/inspectCode.cake"
#load "./scripts/cleanUpCode.cake"
#load "./scripts/dupFinder.cake"

var coverageThreshold = 100;
var solutionFile = GetFiles("./*.sln").First();
var solution = new Lazy<SolutionParserResult>(() => ParseSolution(solutionFile));

// Target - The task you want to start. Runs the Default task if not specified.
var target = Argument("Target", "Default");
var configuration = Argument("Configuration", "Release");

Information($"Running target {target} in configuration {configuration}");

var artifactsDir = Directory("./artifacts");

Task("Information")
    .Does(() =>
    {
        Information($"Solution file {solutionFile} {Environment.NewLine} With following projects :");
        var projects = GetFiles("./**/*.csproj");
        foreach (var p in projects)
        {
            Information($" {p.ToString()} ");
        }
    });


// Deletes the contents of the Artifacts folder if it contains anything from a previous build.
Task("Clean")
    .IsDependentOn("Delete-Artifact")
    .Does(() =>
    {
        // Clean all project directories.
        var projects = GetFiles("./**/*.csproj");
        foreach (var p in projects)
        {
            Information($"Clean project {p.ToString()} ...");
            DotNetCoreClean(p.ToString());
        }
    });

Task("Dependencies-Analyse")
    .Description("Runs the Dependencies Analyser on the solution.")
    .Does(() => 
    {
         var projects = GetFiles("./src/**/*.csproj");
        foreach(var project in projects)
        {            
            var settings = new DependenciesAnalyserSettings
            {
                Project = project.ToString()
            };
            AnalyseDependencies(settings);
        }
    });

Task("Delete-Artifact")
    .Does(() =>
    {
        // Delete artifact output
        if (DirectoryExists(artifactsDir))
        {
            Information($"Delete artifacts in {artifactsDir} ...");
            DeleteDirectory(artifactsDir,  new DeleteDirectorySettings {
                Recursive = true,
                Force = true
            });
        }
    });

// Run dotnet restore to restore all package references.
Task("Restore")
    .Does(() =>
    {
        DotNetCoreRestore(solutionFile.ToString());
    });

// Build using the build configuration specified as an argument.
 Task("Build")
    .Does(() =>
    {
        DotNetCoreBuild(".",
            new DotNetCoreBuildSettings()
            {
                Configuration = configuration,
                ArgumentCustomization = args => args.Append("--no-restore")
            });
    });

// Look under a 'Tests' folder and run dotnet test against all of those projects.
// Then drop the XML test results file in the Artifacts folder at the root.
Task("Test")
    .Description("Run all unit tests within the project.")
    .Does(() =>
    {
        var projects = GetFiles("./test/**/*.csproj");
        foreach(var project in projects)
        {
            Information("Testing project " + project);
            DotNetCoreTest(
                project.ToString(),
                new DotNetCoreTestSettings()
                {
                    Configuration = configuration,
                    NoBuild = true,
					NoRestore = true,
                    ArgumentCustomization = args => args.Append("/p:CollectCoverage=true")
                                             .Append("/p:CoverletOutputFormat=opencover")
                                             .Append("/p:ThresholdType=line")
                                             .Append($"/p:CoverletOutput=../../{artifactsDir}/{System.IO.Path.GetFileNameWithoutExtension(project.ToString())}.xml")
                                             //.Append($"/p:Threshold={coverageThreshold}")
                });
        }
    });

// Publish the app to the /dist folder
Task("PublishWeb")
    .Does(() =>
    {
		var webProjects = solution.Value
			.Projects
			.Where(p => p.Name.EndsWith(".Web"));

		foreach(var project in webProjects)
		{
			Information("Publishing {0}", project.Name);

       		DotNetCorePublish(
				project.Name,
				new DotNetCorePublishSettings()
				{
					Configuration = configuration,
					ArgumentCustomization = args => args.Append("--no-restore"),
				});
		}
    });

// A meta-task that runs all the steps to Build and Test the app
Task("Default")
    .IsDependentOn("Information")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("Build")
    .IsDependentOn("Test");

Task("Validate")
    .Description("Validate code quality using Resharper CLI. tools as well as check for old nuget packages,")
    .IsDependentOn("DupFinder")
    .IsDependentOn("InspectCode");

// The default task to run if none is explicitly specified. In this case, we want
// to run everything starting from Clean, all the way up to Publish.
Task("Full")
    .IsDependentOn("Default")
    .IsDependentOn("Validate")
    .IsDependentOn("PublishWeb");

// Executes the task specified in the target argument.
RunTarget(target);