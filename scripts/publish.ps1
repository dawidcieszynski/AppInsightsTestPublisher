Push-Location $PSScriptRoot/../src

$version = $([System.DateTime]::Now.ToString("yyyy.MM.dd.HHmm"))

Remove-Item .\nupkg\*
dotnet pack -p:PackageVersion=$version
dotnet nuget push ".\nupkg\AppInsightsTestPublisher.*.nupkg" -k "$apikey" -s https://api.nuget.org/v3/index.json
Remove-Item .\nupkg\*

Pop-Location