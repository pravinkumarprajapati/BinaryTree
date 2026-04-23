namespace EmployeeAgent.Data;

public sealed class InMemoryEmployeeRepository : IEmployeeRepository
{
    private static readonly Dictionary<string, Employee> Seed = new(StringComparer.OrdinalIgnoreCase)
    {
        ["E001"] = new Employee(
            Id: "E001",
            FullName: "Aanya Sharma",
            JobTitle: "Senior Software Engineer",
            Department: "Engineering",
            Salary: 9_500m,
            LeaveBalanceDays: 12,
            LastPromotionDate: new DateOnly(2024, 03, 15),
            Email: "aanya.sharma@example.com",
            Phone: "+91-90000-00001",
            Address: "14 MG Road, Bengaluru, KA 560001, India",
            BankAccountLast4: "4821"),

        ["E002"] = new Employee(
            Id: "E002",
            FullName: "Ben Carter",
            JobTitle: "HR Business Partner",
            Department: "Human Resources",
            Salary: 7_200m,
            LeaveBalanceDays: 18,
            LastPromotionDate: new DateOnly(2023, 09, 01),
            Email: "ben.carter@example.com",
            Phone: "+1-415-555-0102",
            Address: "200 Market Street, San Francisco, CA 94103, USA",
            BankAccountLast4: "1107"),

        ["E003"] = new Employee(
            Id: "E003",
            FullName: "Chitra Pillai",
            JobTitle: "Account Executive",
            Department: "Sales",
            Salary: 8_100m,
            LeaveBalanceDays: 5,
            LastPromotionDate: new DateOnly(2025, 01, 20),
            Email: "chitra.pillai@example.com",
            Phone: "+91-90000-00003",
            Address: "88 Marine Drive, Mumbai, MH 400002, India",
            BankAccountLast4: "5540"),

        ["E004"] = new Employee(
            Id: "E004",
            FullName: "Diego Alvarez",
            JobTitle: "Finance Manager",
            Department: "Finance",
            Salary: 10_400m,
            LeaveBalanceDays: 22,
            LastPromotionDate: null,
            Email: "diego.alvarez@example.com",
            Phone: "+34-600-000-004",
            Address: "Calle Gran Via 45, 28013 Madrid, Spain",
            BankAccountLast4: "9032"),

        ["E005"] = new Employee(
            Id: "E005",
            FullName: "Emiko Tanaka",
            JobTitle: "Operations Lead",
            Department: "Operations",
            Salary: 8_800m,
            LeaveBalanceDays: 9,
            LastPromotionDate: new DateOnly(2022, 11, 30),
            Email: "emiko.tanaka@example.com",
            Phone: "+81-3-0000-0005",
            Address: "2-1-1 Shibuya, Tokyo 150-0002, Japan",
            BankAccountLast4: "7768"),
    };

    public Employee? Find(string employeeId) =>
        Seed.TryGetValue(employeeId, out var e) ? e : null;
}
