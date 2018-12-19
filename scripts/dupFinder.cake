#tool "nuget:https://www.nuget.org/api/v2?package=JetBrains.ReSharper.CommandLineTools&version=2018.1.0"

Task("DupFinder")
    .Does(() =>
{
    var settings = new DupFinderSettings() {
        ShowStats = true,
        ShowText = true,
        OutputFile = $"{artifactsDir}/dupfinder.xml",
        ExcludeCodeRegionsByNameSubstring = new string [] { "DupFinder Exclusion" },
        ThrowExceptionOnFindingDuplicates = true
    };
    DupFinder(solutionFile, settings);
});


