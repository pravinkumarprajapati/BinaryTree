using System.ComponentModel;
using EmployeeAgent.Http;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace EmployeeAgent.Plugins;

public sealed class LogAnalyticsPlugin
{
    private readonly LogAnalyticsClient _client;
    private readonly ILogger<LogAnalyticsPlugin> _log;

    public LogAnalyticsPlugin(LogAnalyticsClient client, ILogger<LogAnalyticsPlugin> log)
    {
        _client = client;
        _log = log;
    }

    [KernelFunction]
    [Description("Run the planned Azure Log Analytics query (planning a task) and report whether records were found. Use this when the user says 'plan a task' or 'run the planned query'.")]
    public async Task<string> RunPlannedQuery(CancellationToken cancellationToken = default)
    {
        try
        {
            var ok = await _client.RunDefaultQueryAsync(cancellationToken);
            return ok ? "Record found" : "No records returned.";
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "RunPlannedQuery failed.");
            return "The planned query could not be executed right now.";
        }
    }
}
