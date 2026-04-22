namespace EmployeeAgent.Data;

public sealed class InMemoryEmployeeRepository : IEmployeeRepository
{
    private static readonly Dictionary<string, Employee> Seed = new(StringComparer.OrdinalIgnoreCase)
    {
        ["E001"] = new Employee(
            Id: "E001",
            FullName: "Aanya Sharma",
            Department: "Engineering",
            Salary: 9_500m,
            LeaveBalanceDays: 12,
            LastPromotionDate: new DateOnly(2024, 03, 15),
            Email: "aanya.sharma@example.com",
            Phone: "+91-90000-00001"),

        ["E002"] = new Employee(
            Id: "E002",
            FullName: "Ben Carter",
            Department: "Human Resources",
            Salary: 7_200m,
            LeaveBalanceDays: 18,
            LastPromotionDate: new DateOnly(2023, 09, 01),
            Email: "ben.carter@example.com",
            Phone: "+1-415-555-0102"),

        ["E003"] = new Employee(
            Id: "E003",
            FullName: "Chitra Pillai",
            Department: "Sales",
            Salary: 8_100m,
            LeaveBalanceDays: 5,
            LastPromotionDate: new DateOnly(2025, 01, 20),
            Email: "chitra.pillai@example.com",
            Phone: "+91-90000-00003"),

        ["E004"] = new Employee(
            Id: "E004",
            FullName: "Diego Alvarez",
            Department: "Finance",
            Salary: 10_400m,
            LeaveBalanceDays: 22,
            LastPromotionDate: null,
            Email: "diego.alvarez@example.com",
            Phone: "+34-600-000-004"),

        ["E005"] = new Employee(
            Id: "E005",
            FullName: "Emiko Tanaka",
            Department: "Operations",
            Salary: 8_800m,
            LeaveBalanceDays: 9,
            LastPromotionDate: new DateOnly(2022, 11, 30),
            Email: "emiko.tanaka@example.com",
            Phone: "+81-3-0000-0005"),
    };

    public Employee? Find(string employeeId) =>
        Seed.TryGetValue(employeeId, out var e) ? e : null;
}
