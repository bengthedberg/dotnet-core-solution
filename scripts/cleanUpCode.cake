#tool "nuget:https://www.nuget.org/api/v2?package=JetBrains.ReSharper.CommandLineTools&version=2018.1.0"
var CleanUpCodeExecutable = "./tools/JetBrains.ReSharper.CommandLineTools.2018.1.0/tools/cleanupcode.exe";

private void CleanUpCode(string solution)
{
    StartProcess(CleanUpCodeExecutable, new ProcessSettings {
        Arguments = new ProcessArgumentBuilder()
            .Append(solution)
        }
    );
}

Task("CleanUpCode")
    .Does(() =>
    {
        CleanUpCode(solutionFile.ToString());
    });
