using System.ComponentModel;
using System.Globalization;
using EmployeeAgent.Data;
using EmployeeAgent.Security;
using Microsoft.SemanticKernel;

namespace EmployeeAgent.Plugins;

public sealed class EmployeePlugin
{
    private readonly IEmployeeRepository _repo;

    public EmployeePlugin(IEmployeeRepository repo) => _repo = repo;

    [KernelFunction]
    [Description("Get an employee's monthly salary by employee ID.")]
    public string GetSalary(
        [Description("Employee ID like E001")] string employeeId)
    {
        var e = Lookup(employeeId);
        return $"Salary for {e.FullName}: {e.Salary.ToString("C", CultureInfo.InvariantCulture)}";
    }

    [KernelFunction]
    [Description("Get an employee's remaining leave balance, in days, by employee ID.")]
    public string GetLeaveBalance(
        [Description("Employee ID like E001")] string employeeId)
    {
        var e = Lookup(employeeId);
        return $"{e.FullName} has {e.LeaveBalanceDays} leave day(s) remaining.";
    }

    [KernelFunction]
    [Description("Get the department an employee belongs to by employee ID.")]
    public string GetDepartment(
        [Description("Employee ID like E001")] string employeeId)
    {
        var e = Lookup(employeeId);
        return $"{e.FullName} works in {e.Department}.";
    }

    [KernelFunction]
    [Description("Get the date of an employee's last promotion by employee ID.")]
    public string GetLastPromotion(
        [Description("Employee ID like E001")] string employeeId)
    {
        var e = Lookup(employeeId);
        return e.LastPromotionDate is null
            ? $"{e.FullName} has never been promoted."
            : $"{e.FullName} was last promoted on {e.LastPromotionDate.Value:yyyy-MM-dd}.";
    }

    [KernelFunction]
    [Description("Get an employee's basic personal info (name, email, phone) by employee ID. Does not return sensitive fields like SSN or DOB.")]
    public string GetPersonalInfo(
        [Description("Employee ID like E001")] string employeeId)
    {
        var e = Lookup(employeeId);
        return $"{e.FullName} | {e.Email} | {e.Phone}";
    }

    private Employee Lookup(string employeeId)
    {
        var id = InputSanitizer.NormalizeEmployeeId(employeeId);
        return _repo.Find(id)
            ?? throw new KeyNotFoundException($"No employee found with ID {id}.");
    }
}
