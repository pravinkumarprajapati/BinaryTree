using System.ComponentModel;
using System.Globalization;
using EmployeeAgent.Data;
using EmployeeAgent.Security;
using Microsoft.SemanticKernel;

namespace EmployeeAgent.Plugins;

public sealed class HRPlugin
{
    private readonly IEmployeeRepository _repo;

    public HRPlugin(IEmployeeRepository repo) => _repo = repo;

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
    [Description("Get an employee's email address and phone number by employee ID.")]
    public string GetContactInfo(
        [Description("Employee ID like E001")] string employeeId)
    {
        var e = Lookup(employeeId);
        return $"{e.FullName} | {e.Email} | {e.Phone}";
    }

    [KernelFunction]
    [Description("Get an employee's postal address by employee ID.")]
    public string GetAddress(
        [Description("Employee ID like E001")] string employeeId)
    {
        var e = Lookup(employeeId);
        return $"{e.FullName}'s address: {e.Address}";
    }

    [KernelFunction]
    [Description("Get the masked last 4 digits of an employee's bank account by employee ID. Never returns the full account number.")]
    public string GetBankAccountLast4(
        [Description("Employee ID like E001")] string employeeId)
    {
        var e = Lookup(employeeId);
        return $"{e.FullName}'s bank account ends in ****{e.BankAccountLast4}";
    }

    private Employee Lookup(string employeeId)
    {
        var id = InputSanitizer.NormalizeEmployeeId(employeeId);
        return _repo.Find(id)
            ?? throw new KeyNotFoundException($"No employee found with ID {id}.");
    }
}
