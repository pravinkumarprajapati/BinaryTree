namespace EmployeeAgent.Security;

public sealed class PromptInjectionException : Exception
{
    public PromptInjectionException(string message) : base(message) { }
}
