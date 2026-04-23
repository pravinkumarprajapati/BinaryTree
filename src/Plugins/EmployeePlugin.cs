using System.ComponentModel;
using EmployeeAgent.Data;
using EmployeeAgent.Security;
using Microsoft.SemanticKernel;

namespace EmployeeAgent.Plugins;

public sealed class EmployeePlugin
{
    private readonly IEmployeeRepository _repo;

    public EmployeePlugin(IEmployeeRepository repo) => _repo = repo;

    [KernelFunction]
    [Description("Get an employee's job title by employee ID.")]
    public string GetJobTitle(
        [Description("Employee ID like E001")] string employeeId)
    {
        var e = Lookup(employeeId);
        return $"{e.FullName}'s job title is {e.JobTitle}.";
    }

    [KernelFunction]
    [Description("Get the department an employee belongs to by employee ID.")]
    public string GetDepartment(
        [Description("Employee ID like E001")] string employeeId)
    {
        var e = Lookup(employeeId);
        return $"{e.FullName} works in {e.Department}.";
    }

    private Employee Lookup(string employeeId)
    {
        var id = InputSanitizer.NormalizeEmployeeId(employeeId);
        return _repo.Find(id)
            ?? throw new KeyNotFoundException($"No employee found with ID {id}.");
    }
}
