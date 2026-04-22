using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace EmployeeAgent.Security;

public sealed class AuditFilter : IFunctionInvocationFilter
{
    private readonly ILogger<AuditFilter> _log;

    public AuditFilter(ILogger<AuditFilter> log) => _log = log;

    public async Task OnFunctionInvocationAsync(
        FunctionInvocationContext context,
        Func<FunctionInvocationContext, Task> next)
    {
        var sw = Stopwatch.StartNew();
        var name = $"{context.Function.PluginName}.{context.Function.Name}";
        var args = string.Join(",", context.Arguments.Select(kv => $"{kv.Key}={kv.Value}"));

        try
        {
            await next(context);
            sw.Stop();
            _log.LogInformation(
                "TOOL OK  {Function}({Args}) [{Ms} ms]",
                name, args, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _log.LogWarning(
                ex,
                "TOOL ERR {Function}({Args}) [{Ms} ms]: {Message}",
                name, args, sw.ElapsedMilliseconds, ex.Message);
            throw;
        }
    }
}
