# AppInsightsTestPublisher

This tool is used do publish tests results from TRX files to App Insights to be used in Grafana dashboards.

Tool will try to find the App Insightsc connection string in the environment variables, especially 
`ApplicationInsights:ConnectionString` or `APPINSIGHTS_CONNECTIONSTRING`

## Usage

```
dotnet new tool-manifest
dotnet tool install AppInsightsTestPublisher --version 2024.4.18.1030
dotnet appi-tests-publish src/output/
```

## Tool update / publish

Run:

```
$apikey="<your nuget api key>"
./scripts/publish.ps1
```

You can also upload the `src\Tools\AppInsightsTestPublisher\nupkg\AppInsightsTestPublisher.1.0.0.nupkg` file at https://www.nuget.org/packages/manage/upload manually

Warning: If you want the package to be unlisted you have to change it on the nuget.org