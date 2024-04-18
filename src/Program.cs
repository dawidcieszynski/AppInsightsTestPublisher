using System.Reflection;
using System.Xml.Linq;
using AppInsightsTestPublisher;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;

var commandLineArgs = Environment.GetCommandLineArgs();
if (commandLineArgs.Length < 1)
{
    var versionString = Assembly.GetEntryAssembly()?
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
        .InformationalVersion;

    Console.WriteLine($"appi-tests-publish v{versionString}");
    Console.WriteLine("-------------");
    Console.WriteLine("\nUsage:");
    Console.WriteLine("  appi-tests-publish <directory>");
    return;
}

var directoryPath = commandLineArgs[1];

if (directoryPath == "dumpenv")
{
    Console.WriteLine("Environment: ");
    var environmentVariables = Environment.GetEnvironmentVariables();
    foreach (var key in environmentVariables.Keys) Console.WriteLine($"{key}: {environmentVariables[key]}");
    return;
}

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

var configuration = builder.Build();

var connectionString = configuration["ApplicationInsights:ConnectionString"] ??
                       Environment.GetEnvironmentVariable("APPINSIGHTS_CONNECTIONSTRING");

Console.WriteLine("Using ApplicationInsights: " + connectionString);

var telemetryClient = new TelemetryClient(new TelemetryConfiguration
{
    ConnectionString = connectionString
});

var publishDate = DateTimeOffset.UtcNow;

var trxFiles = Directory.GetFiles(directoryPath, "*.trx", SearchOption.AllDirectories);

foreach (var trxFile in trxFiles)
{
    Console.WriteLine($"Processing {trxFile}");

    var xml = File.ReadAllText(trxFile);

    var xDoc = XDocument.Parse(xml);

    XNamespace ns = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";

    var unitTestResults = xDoc.Root?.Descendants(ns + "UnitTestResult")
        .Select(unitTestResultElement =>
        {
            var errorInfoElement = unitTestResultElement.Element(ns + "Output")?.Element(ns + "ErrorInfo");
            var message = errorInfoElement?.Element(ns + "Message")?.Value;
            var stackTrace = errorInfoElement?.Element(ns + "StackTrace")?.Value;

            var testName = unitTestResultElement.Attribute("testName")?.Value;
            var testLocationParts = testName?.Split('(').FirstOrDefault()?.Split('.').Reverse().ToArray();
            var testMethodName = testLocationParts?.FirstOrDefault();
            var testClassName = testLocationParts?.Skip(1).FirstOrDefault();
            var testProjectName =
                string.Join(".", testLocationParts?.Skip(2).Take(2).Reverse() ?? Array.Empty<string>());

            return new UnitTestResult
            {
                PublishDate = publishDate.ToString("O"),
                ExecutionId = unitTestResultElement.Attribute("executionId")?.Value,
                TestId = unitTestResultElement.Attribute("testId")?.Value,
                TestName = testName,
                ComputerName = unitTestResultElement.Attribute("computerName")?.Value,
                Duration = unitTestResultElement.Attribute("duration")?.Value,
                StartTime = unitTestResultElement.Attribute("startTime")?.Value,
                EndTime = unitTestResultElement.Attribute("endTime")?.Value,
                TestType = unitTestResultElement.Attribute("endTime")?.Value,
                Outcome = unitTestResultElement.Attribute("outcome")?.Value,
                TestListId = unitTestResultElement.Attribute("testListId")?.Value,
                RelativeResultsDirectory = unitTestResultElement.Attribute("relativeResultsDirectory")?.Value,
                MethodName = testMethodName,
                ClassName = testClassName,
                ProjectName = testProjectName,
                BuildId = Environment.GetEnvironmentVariable("BUILD_BUILDID"),
                Message = message,
                StackTrace = stackTrace
            };
        }) ?? new List<UnitTestResult>();

    foreach (var unitTestResult in unitTestResults)
    {
        Console.WriteLine(unitTestResult);
        SendEvent(unitTestResult, telemetryClient);
    }
}

telemetryClient.Flush();

void SendEvent(
    UnitTestResult unitTestResult,
    TelemetryClient client
)
{
    var properties = new Dictionary<string, string>();
    var dataType = unitTestResult.GetType();
    foreach (var property in dataType.GetProperties())
    {
        var value = property.GetValue(unitTestResult);
        properties[property.Name] = value?.ToString() ?? string.Empty;
    }

    client.TrackEvent("UnitTestResult", properties);
}