Param
(
    [Parameter(Mandatory = $true)]
    [String]$SolutionName
)

$DestinationFolder = (Resolve-Path .\).Path

Write-Host "Creating solution $SolutionName in $DestinationFolder"
Write-Host ""
Write-Host "Creating folder $DestinationFolder\$SolutionName"
New-Item -ItemType directory -Path $DestinationFolder\$SolutionName | Out-Null
New-Item -ItemType directory -Path $DestinationFolder\$SolutionName\scripts | Out-Null
New-Item -ItemType directory -Path $DestinationFolder\$SolutionName\doc | Out-Null
New-Item -ItemType directory -Path $DestinationFolder\$SolutionName\tools | Out-Null
Set-Location -Path .\$SolutionName

Write-Host  $PSScriptRoot

## Setup Git
Write-Host "Initialise git repository"
git init

Write-Host "Add relevant .gitignore file"
## Add the relevant .gitignore files. In this case we will use one for Visual Studio with some extra for Visual Studio Code and Cake. Use https://www.gitignore.io/
(curl https://www.gitignore.io/api/visualstudio).Content -split "`n" -join "`r`n" | set-content .gitignore
(curl https://www.gitignore.io/api/visualstudiocode).Content -split "`n" -join "`r`n" | add-content .gitignore
(curl https://www.gitignore.io/api/cake).Content -split "`n" -join "`r`n" | add-content .gitignore

Write-Host "Add the solution file $SolutionName.sln"
dotnet new sln --name $SolutionName

Write-Host "Add the editor config file will helps developers define and maintain consistent coding styles"
(curl https://raw.githubusercontent.com/dotnet/roslyn/master/.editorconfig).Content   -split "`n" -join "`r`n" | set-content .editorconfig

Write-Host "Add a cross-platform build automation tool."
Invoke-WebRequest https://cakebuild.net/download/bootstrapper/windows -OutFile build.ps1
Invoke-WebRequest https://cakebuild.net/download/bootstrapper/linux -OutFile build.sh 
git update-index --add --chmod=+x build.sh

Copy-Item $PSScriptRoot\build.cake $DestinationFolder\$SolutionName
Copy-Item $PSScriptRoot\scripts\cleanUpCode.cake $DestinationFolder\$SolutionName\scripts
Copy-Item $PSScriptRoot\scripts\inspectCode.cake $DestinationFolder\$SolutionName\scripts
Copy-Item $PSScriptRoot\scripts\dupFinder.cake $DestinationFolder\$SolutionName\scripts
Copy-Item $PSScriptRoot\.DotSettings $DestinationFolder\$SolutionName\$SolutionName.sln.DotSettings
Copy-Item $PSScriptRoot\BuildConfig.md $DestinationFolder\$SolutionName\doc
Copy-Item $PSScriptRoot\README.md $DestinationFolder\$SolutionName
Copy-Item $PSScriptRoot\packages.config $DestinationFolder\$SolutionName\tools

Write-Host "Create a web"
dotnet new webapi --name "$SolutionName.Web" --output "src\$SolutionName.Web"
dotnet sln add ".\src\$SolutionName.Web\$SolutionName.Web.csproj"
dotnet add ".\src\$SolutionName.Web\$SolutionName.Web.csproj" package Newtonsoft.Json
dotnet new mstest --name "$SolutionName.Web.Test" --output "test\$SolutionName.Web.Test"
dotnet sln add ".\test\$SolutionName.Web.Test\$SolutionName.Web.Test.csproj"
dotnet add ".\test\$SolutionName.Web.Test\$SolutionName.Web.Test.csproj" package coverlet.msbuild 
dotnet add ".\test\$SolutionName.Web.Test\$SolutionName.Web.Test.csproj" reference ".\src\$SolutionName.Web\$SolutionName.Web.csproj"

Write-Host "Create a core library"
dotnet new classlib --name "$SolutionName.Core" --output "src\$SolutionName.Core"
dotnet sln add ".\src\$SolutionName.Core\$SolutionName.Core.csproj"
dotnet add ".\src\$SolutionName.Core\$SolutionName.Core.csproj" package Newtonsoft.Json
dotnet new mstest --name "$SolutionName.Core.Test" --output "test\$SolutionName.Core.Test"
dotnet sln add ".\test\$SolutionName.Core.Test\$SolutionName.Core.Test.csproj"
dotnet add ".\test\$SolutionName.Core.Test\$SolutionName.Core.Test.csproj" package coverlet.msbuild 
dotnet add ".\test\$SolutionName.Core.Test\$SolutionName.Core.Test.csproj" reference ".\src\$SolutionName.Core\$SolutionName.Core.csproj"

Write-Host "Create a infrastructure library"
dotnet new classlib --name "$SolutionName.Infrastructure" --output "src\$SolutionName.Infrastructure"
dotnet sln add ".\src\$SolutionName.Infrastructure\$SolutionName.Infrastructure.csproj"
dotnet add ".\src\$SolutionName.Infrastructure\$SolutionName.Infrastructure.csproj" package Newtonsoft.Json
dotnet new mstest --name "$SolutionName.Infrastructure.Test" --output "test\$SolutionName.Infrastructure.Test"
dotnet sln add ".\test\$SolutionName.Infrastructure.Test\$SolutionName.Infrastructure.Test.csproj"
dotnet add  ".\test\$SolutionName.Infrastructure.Test\$SolutionName.Infrastructure.Test.csproj" package coverlet.msbuild
dotnet add ".\test\$SolutionName.Infrastructure.Test\$SolutionName.Infrastructure.Test.csproj" reference ".\src\$SolutionName.Infrastructure\$SolutionName.Infrastructure.csproj"


& .\build.ps1 -target CleanUpCode
& .\build.ps1
& .\build.ps1 -target InspectCode
& .\build.ps1 -target DupFinder

Set-Location -Path $DestinationFolder
