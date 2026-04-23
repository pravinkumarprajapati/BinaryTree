using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EmployeeAgent.Http;

public sealed class LogAnalyticsClient
{
    private readonly HttpClient _http;
    private readonly LogAnalyticsOptions _options;
    private readonly TokenCredential _credential;
    private readonly ILogger<LogAnalyticsClient> _log;

    public LogAnalyticsClient(
        HttpClient http,
        IOptions<LogAnalyticsOptions> options,
        ILogger<LogAnalyticsClient> log)
    {
        _http = http;
        _options = options.Value;
        _credential = new DefaultAzureCredential();
        _log = log;
    }

    public async Task<bool> RunDefaultQueryAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.Endpoint))
            throw new InvalidOperationException("LogAnalytics:Endpoint is not configured.");

        /*var token = await _credential.GetTokenAsync(
            new TokenRequestContext(new[] { _options.Scope }),
            cancellationToken);*/

        var payload = JsonSerializer.Serialize(new { query = _options.Query });

        using var request = new HttpRequestMessage(HttpMethod.Get, _options.Endpoint)
        {            
        };
        //request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        request.Headers.Add("X-Api-Key", "OCO9dWICV+YvbTdbX+DRKA==bTxwBdyhHM922D3m");
        using var response = await _http.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _log.LogWarning(
                "Log Analytics call failed: {Status} {Body}",
                (int)response.StatusCode, Truncate(body, 500));
            return false;
        }

        return true;
    }

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..max] + "...";
}
