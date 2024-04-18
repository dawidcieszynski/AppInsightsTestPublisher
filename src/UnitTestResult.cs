namespace AppInsightsTestPublisher;

internal class UnitTestResult
{
    public string? ExecutionId { get; set; }
    public string? TestId { get; set; }
    public string? TestName { get; set; }
    public string? Outcome { get; set; }
    public string? MethodName { get; set; }
    public string? ClassName { get; set; }
    public string? ProjectName { get; set; }
    public string? ComputerName { get; set; }
    public string? Duration { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public string? TestType { get; set; }
    public string? TestListId { get; set; }
    public string? RelativeResultsDirectory { get; set; }
    public string? PublishDate { get; set; }
    public string? BuildId { get; set; }
    public string? Message { get; set; }
    public string? StackTrace { get; set; }

    public override string ToString()
    {
        return $"ExecutionId: {ExecutionId}\nTestId: {TestId}\nTestName: {TestName}\nOutcome: {Outcome}\n";
    }
}