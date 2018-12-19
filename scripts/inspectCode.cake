#tool "nuget:https://www.nuget.org/api/v2?package=JetBrains.ReSharper.CommandLineTools&version=2018.1.0"

Task("InspectCode")
    .Does(() =>
{
     var msBuildProperties = new Dictionary<string, string>();
     msBuildProperties.Add("configuration", configuration);
     msBuildProperties.Add("platform", "AnyCPU");

    InspectCode(solutionFile.ToString(), new InspectCodeSettings {
     SolutionWideAnalysis = true,
     Profile = solutionFile.ToString() + ".DotSettings",
     MsBuildProperties = msBuildProperties,
     OutputFile = $"{artifactsDir}/inspectcode.xml",
     ThrowExceptionOnFindingViolations = true });
});

