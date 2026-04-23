using EmployeeAgent.Agent.Prompts;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

namespace EmployeeAgent.Agent.Orchestrator;

public sealed class AgentOrchestrator
{
    private readonly ChatCompletionAgent _router;
    private readonly ChatCompletionAgent _employeeAgent;
    private readonly ChatCompletionAgent _hrAgent;
    private readonly ILogger<AgentOrchestrator> _log;

    public AgentOrchestrator(
        ChatCompletionAgent router,
        ChatCompletionAgent employeeAgent,
        ChatCompletionAgent hrAgent,
        ILogger<AgentOrchestrator> log)
    {
        _router        = router;
        _employeeAgent = employeeAgent;
        _hrAgent       = hrAgent;
        _log           = log;
    }

    public async Task<string> HandleTurnAsync(
        ChatHistory history,
        string userMessage,
        CancellationToken cancellationToken = default)
    {
        history.AddUserMessage(userMessage);

        var label = await ClassifyAsync(userMessage, cancellationToken);
        _log.LogInformation("Router classified turn as {Label}.", label);

        if (label == RouterPrompt.LabelRefuse)
        {
            history.AddAssistantMessage(RouterPrompt.Refusal);
            return RouterPrompt.Refusal;
        }

        var specialist = label == RouterPrompt.LabelHR ? _hrAgent : _employeeAgent;

        var scratch = new ChatHistory();
        foreach (var msg in history)
        {
            scratch.Add(msg);
        }

        string? reply = null;
        await foreach (var message in specialist.InvokeAsync(scratch, cancellationToken: cancellationToken))
        {
            if (!string.IsNullOrWhiteSpace(message.Content))
            {
                reply = message.Content;
            }
        }

        reply ??= RouterPrompt.Refusal;
        history.AddAssistantMessage(reply);
        return reply;
    }

    private async Task<string> ClassifyAsync(string userMessage, CancellationToken cancellationToken)
    {
        var routerHistory = new ChatHistory();
        routerHistory.AddUserMessage(userMessage);

        string? raw = null;
        await foreach (var message in _router.InvokeAsync(routerHistory, cancellationToken: cancellationToken))
        {
            if (!string.IsNullOrWhiteSpace(message.Content))
            {
                raw = message.Content;
            }
        }

        var normalized = (raw ?? string.Empty)
            .Trim()
            .Trim('.', ',', '"', '\'', '`')
            .ToUpperInvariant();

        return normalized switch
        {
            RouterPrompt.LabelEmployee => RouterPrompt.LabelEmployee,
            RouterPrompt.LabelHR       => RouterPrompt.LabelHR,
            _                          => RouterPrompt.LabelRefuse,
        };
    }
}
