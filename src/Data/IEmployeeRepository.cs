namespace EmployeeAgent.Data;

public interface IEmployeeRepository
{
    Employee? Find(string employeeId);
}
