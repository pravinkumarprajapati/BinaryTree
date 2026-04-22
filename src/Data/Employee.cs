namespace EmployeeAgent.Data;

public sealed record Employee(
    string Id,
    string FullName,
    string Department,
    decimal Salary,
    int LeaveBalanceDays,
    DateOnly? LastPromotionDate,
    string Email,
    string Phone);
