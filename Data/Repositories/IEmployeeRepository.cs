using CSMTutorial.Models;

namespace CSMTutorial.Data.Repositories;

public interface IEmployeeRepository
{
    // Get methods with filters
    Task<IEnumerable<Employee>> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchTerm = null,
        int? companyId = null,
        int? departmentId = null,
        string? gender = null,
        string? status = null);

    Task<int> GetTotalCountAsync(
        string? searchTerm = null,
        int? companyId = null,
        int? departmentId = null,
        string? gender = null,
        string? status = null);

    Task<Employee?> GetByCodeAsync(string employeeCode);
    Task<bool> ExistsAsync(string employeeCode);

    // Existing codes/contacts check
    Task<IEnumerable<string>> GetExistingCodesAsync(IEnumerable<string> codes);
    Task<IEnumerable<string>> GetExistingContactNumbersAsync(IEnumerable<string> contactNumbers);
    Task<IEnumerable<ExistingEmployeeInfo>> GetEmployeesByContactNumbersAsync(IEnumerable<string> contactNumbers);

    // CRUD operations
    Task<int> InsertAsync(Employee employee);
    Task<int> UpdateAsync(Employee employee);
    Task<int> DeleteAsync(string employeeCode);

    // Upsert operations
    Task<(int EmployeeId, string EmployeeCode, string Operation)> UpsertByContactNoAsync(Employee employee);
    Task<(int Inserted, int Updated)> UpsertBatchByContactNoAsync(IEnumerable<Employee> employees);

    // Export
    Task<IEnumerable<Employee>> GetForExportAsync( int? companyId = null, int? departmentId = null, string? status = null);

    // Dropdowns
    Task<IEnumerable<CompanyDropdown>> GetCompaniesDropdownAsync();
    Task<IEnumerable<DepartmentDropdown>> GetDepartmentsDropdownAsync(int? companyId = null);
    Task<IEnumerable<CategoryDropdown>> GetCategoriesDropdownAsync();
}