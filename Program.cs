using EmployeeAgent.Agent;
using EmployeeAgent.Data;
using EmployeeAgent.Http;
using EmployeeAgent.Plugins;
using EmployeeAgent.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Polly;
using Polly.Extensions.Http;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables();

builder.Services.Configure<AzureOpenAIOptions>(
    builder.Configuration.GetSection(AzureOpenAIOptions.SectionName));
builder.Services.Configure<LogAnalyticsOptions>(
    builder.Configuration.GetSection(LogAnalyticsOptions.SectionName));

builder.Services.AddSingleton<IEmployeeRepository, InMemoryEmployeeRepository>();

builder.Services.AddHttpClient<LogAnalyticsClient>(http =>
{
    http.Timeout = TimeSpan.FromSeconds(10);
})
.AddPolicyHandler(HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))));

builder.Services.AddSingleton<EmployeePlugin>();
builder.Services.AddSingleton<LogAnalyticsPlugin>();
builder.Services.AddSingleton<AuditFilter>();

builder.Services.AddSingleton(sp => AgentFactory.Build(sp));

using var host = builder.Build();

var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Program");
var kernel = host.Services.GetRequiredService<Kernel>();
var chat   = kernel.GetRequiredService<IChatCompletionService>();

var settings = new AzureOpenAIPromptExecutionSettings
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
    Temperature = 0.2,
};

var history = new ChatHistory(SystemPrompt.Text);

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
        Console.WriteLine($"HR-Buddy: {SystemPrompt.Refusal}");
        continue;
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine($"HR-Buddy: {ex.Message}");
        continue;
    }

    history.AddUserMessage(clean);

    try
    {
        var reply = await chat.GetChatMessageContentAsync(history, settings, kernel);
        Console.WriteLine($"HR-Buddy: {reply.Content}");
        history.Add(reply);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Chat turn failed.");
        history.RemoveAt(history.Count - 1);
        Console.WriteLine("HR-Buddy: Sorry, something went wrong. Please try again.");
    }
}
