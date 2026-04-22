namespace EmployeeAgent.Http;

public sealed class LogAnalyticsOptions
{
    public const string SectionName = "LogAnalytics";

    public string Endpoint { get; set; } = "";
    public string Scope    { get; set; } = "https://api.loganalytics.io/.default";
    public string Query    { get; set; } = "AzureActivity | summarize count() by Category";
}
