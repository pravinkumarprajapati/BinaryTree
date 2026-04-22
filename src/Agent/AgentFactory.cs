using EmployeeAgent.Plugins;
using EmployeeAgent.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

namespace EmployeeAgent.Agent;

public static class AgentFactory
{
    public static Kernel Build(IServiceProvider sp)
    {
        var aoai = sp.GetRequiredService<IOptions<AzureOpenAIOptions>>().Value;

        if (string.IsNullOrWhiteSpace(aoai.Endpoint) ||
            string.IsNullOrWhiteSpace(aoai.ApiKey) ||
            string.IsNullOrWhiteSpace(aoai.Deployment))
        {
            throw new InvalidOperationException(
                "AzureOpenAI Endpoint/ApiKey/Deployment are not configured. " +
                "Set them via 'dotnet user-secrets set AzureOpenAI:Endpoint ...' etc.");
        }

        var builder = Kernel.CreateBuilder();

        builder.Services.AddSingleton(sp.GetRequiredService<ILoggerFactory>());

        builder.AddAzureOpenAIChatCompletion(
            deploymentName: aoai.Deployment,
            endpoint: aoai.Endpoint,
            apiKey: aoai.ApiKey);

        builder.Plugins.AddFromObject(sp.GetRequiredService<EmployeePlugin>(), "EmployeePlugin");
        builder.Plugins.AddFromObject(sp.GetRequiredService<LogAnalyticsPlugin>(), "LogAnalyticsPlugin");

        var kernel = builder.Build();

        kernel.FunctionInvocationFilters.Add(sp.GetRequiredService<AuditFilter>());

        return kernel;
    }
}
