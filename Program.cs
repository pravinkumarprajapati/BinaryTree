using EmployeeAgent.Agent;
using EmployeeAgent.Agent.Orchestrator;
using EmployeeAgent.Data;
using EmployeeAgent.Plugins;
using EmployeeAgent.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables();

builder.Services.Configure<AzureOpenAIOptions>(
    builder.Configuration.GetSection(AzureOpenAIOptions.SectionName));

builder.Services.AddSingleton<IEmployeeRepository, InMemoryEmployeeRepository>();

builder.Services.AddSingleton<EmployeePlugin>();
builder.Services.AddSingleton<HRPlugin>();
builder.Services.AddSingleton<AuditFilter>();

builder.Services.AddSingleton(sp => AgentFactory.Build(sp));

using var host = builder.Build();

var logger       = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Program");
var orchestrator = host.Services.GetRequiredService<AgentOrchestrator>();

var history = new ChatHistory();

Console.WriteLine("HR-Buddy is ready. Ask about employees (E001-E005). Type 'exit' or press Enter on an empty line to quit.");

while (true)
{
    Console.Write("You: ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input)) break;
    if (string.Equals(input.Trim(), "exit", StringComparison.OrdinalIgnoreCase)) break;

    string clean;
    try
    {
        clean = InputSanitizer.Clean(input);
        PromptInjectionGuard.Check(clean);
    }
    catch (PromptInjectionException)
    {
        Console.WriteLine("HR-Buddy: I can only help with employee-related questions.");
        continue;
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine($"HR-Buddy: {ex.Message}");
        continue;
    }

    try
    {
        var reply = await orchestrator.HandleTurnAsync(history, clean);
        Console.WriteLine($"HR-Buddy: {reply}");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Chat turn failed.");
        if (history.Count > 0 && history[^1].Role == AuthorRole.User)
        {
            history.RemoveAt(history.Count - 1);
        }
        Console.WriteLine("HR-Buddy: Sorry, something went wrong. Please try again.");
    }
}
