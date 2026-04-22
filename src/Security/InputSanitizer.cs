using System.Text;
using System.Text.RegularExpressions;

namespace EmployeeAgent.Security;

public static class InputSanitizer
{
    public const int MaxUserInputChars = 2_000;

    private static readonly Regex EmployeeIdRegex =
        new(@"^E\d{3,6}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static string Clean(string input)
    {
        if (input is null) throw new ArgumentNullException(nameof(input));

        if (input.Length > MaxUserInputChars)
            throw new ArgumentException(
                $"Input exceeds {MaxUserInputChars} characters.", nameof(input));

        var sb = new StringBuilder(input.Length);
        foreach (var ch in input)
        {
            if (ch == '\n' || ch == '\r' || ch == '\t' || !char.IsControl(ch))
                sb.Append(ch);
        }
        return sb.ToString().Trim();
    }

    public static string NormalizeEmployeeId(string employeeId)
    {
        if (string.IsNullOrWhiteSpace(employeeId))
            throw new ArgumentException("Employee ID is required.", nameof(employeeId));

        var trimmed = employeeId.Trim().ToUpperInvariant();

        if (!EmployeeIdRegex.IsMatch(trimmed))
            throw new ArgumentException(
                "Employee ID must look like E001 (letter E followed by 3-6 digits).",
                nameof(employeeId));

        return trimmed;
    }
}
