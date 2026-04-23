using EmployeeAgent.Agent.Orchestrator;
using EmployeeAgent.Agent.Prompts;
using EmployeeAgent.Plugins;
using EmployeeAgent.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace EmployeeAgent.Agent;

public static class AgentFactory
{
    public static AgentOrchestrator Build(IServiceProvider sp)
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

        var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
        var auditFilter   = sp.GetRequiredService<AuditFilter>();

        var specialistSettings = new AzureOpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            Temperature = 0.2,
        };

        var routerSettings = new AzureOpenAIPromptExecutionSettings
        {
            Temperature = 0.0,
            MaxTokens = 4,
        };

        var employeeKernel = BuildKernel(aoai, loggerFactory, auditFilter,
            sp.GetRequiredService<EmployeePlugin>(), "EmployeePlugin");

        var hrKernel = BuildKernel(aoai, loggerFactory, auditFilter,
            sp.GetRequiredService<HRPlugin>(), "HRPlugin");

        var routerKernel = BuildBareKernel(aoai, loggerFactory);

        var employeeAgent = new ChatCompletionAgent
        {
            Name         = "EmployeeAgent",
            Instructions = EmployeePrompt.Text,
            Kernel       = employeeKernel,
            Arguments    = new KernelArguments(specialistSettings),
        };

        var hrAgent = new ChatCompletionAgent
        {
            Name         = "HRAgent",
            Instructions = HRPrompt.Text,
            Kernel       = hrKernel,
            Arguments    = new KernelArguments(specialistSettings),
        };

        var routerAgent = new ChatCompletionAgent
        {
            Name         = "RouterAgent",
            Instructions = RouterPrompt.Text,
            Kernel       = routerKernel,
            Arguments    = new KernelArguments(routerSettings),
        };

        return new AgentOrchestrator(
            routerAgent,
            employeeAgent,
            hrAgent,
            loggerFactory.CreateLogger<AgentOrchestrator>());
    }

    private static Kernel BuildKernel(
        AzureOpenAIOptions aoai,
        ILoggerFactory loggerFactory,
        AuditFilter auditFilter,
        object pluginInstance,
        string pluginName)
    {
        var builder = Kernel.CreateBuilder();
        builder.Services.AddSingleton(loggerFactory);
        builder.AddAzureOpenAIChatCompletion(
            deploymentName: aoai.Deployment,
            endpoint: aoai.Endpoint,
            apiKey: aoai.ApiKey);
        builder.Plugins.AddFromObject(pluginInstance, pluginName);

        var kernel = builder.Build();
        kernel.FunctionInvocationFilters.Add(auditFilter);
        return kernel;
    }

    private static Kernel BuildBareKernel(
        AzureOpenAIOptions aoai,
        ILoggerFactory loggerFactory)
    {
        var builder = Kernel.CreateBuilder();
        builder.Services.AddSingleton(loggerFactory);
        builder.AddAzureOpenAIChatCompletion(
            deploymentName: aoai.Deployment,
            endpoint: aoai.Endpoint,
            apiKey: aoai.ApiKey);
        return builder.Build();
    }
}
