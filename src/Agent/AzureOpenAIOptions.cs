namespace EmployeeAgent.Agent;

public sealed class AzureOpenAIOptions
{
    public const string SectionName = "AzureOpenAI";

    public string Endpoint   { get; set; } = "";
    public string ApiKey     { get; set; } = "";
    public string Deployment { get; set; } = "gpt-5.4-mini-1";
}
