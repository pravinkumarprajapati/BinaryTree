using System.Text.RegularExpressions;

namespace EmployeeAgent.Security;

public static class PromptInjectionGuard
{
    private static readonly Regex[] Denylist =
    {
        new(@"\bignore\s+(all\s+)?previous\b",     RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"\bsystem\s+prompt\b",                RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"\breveal\s+(your|the)\b",            RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"\bact\s+as\s+",                      RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"\bdrop\s+table\b",                   RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"```\s*(bash|sh|powershell|cmd)",     RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"\bdump\s+secrets?\b",                RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"\bjailbreak\b",                      RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"\boverride\s+your\s+(rules|instructions)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled),
    };

    public static void Check(string input)
    {
        if (string.IsNullOrEmpty(input)) return;

        foreach (var rule in Denylist)
        {
            if (rule.IsMatch(input))
                throw new PromptInjectionException(
                    "Input rejected: looks like a prompt-injection attempt.");
        }
    }
}
