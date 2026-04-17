using CSMTutorial.Models;
using Dapper;
using System.Data;

namespace CSMTutorial.Data.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly DapperContext _context;
    private readonly ILogger<EmployeeRepository> _logger;

    public EmployeeRepository(DapperContext context, ILogger<EmployeeRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Employee>> GetAllAsync( int pageNumber = 1, int pageSize = 10, string? searchTerm = null, int? companyId = null, int? departmentId = null, string? gender = null, string? status = null)
    {
        try
        {
            using var connection = _context.CreateConnection();

            var parameters = new
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                CompanyId = companyId,
                DepartmentId = departmentId,
                Gender = gender,
                Status = status
            };

            _logger.LogInformation("Calling sp_GetEmployees with params: {@Params}", parameters);

            var result = await connection.QueryAsync<Employee>(
                "sp_GetEmployees",
                parameters,
                commandType: CommandType.StoredProcedure);

            _logger.LogInformation("sp_GetEmployees returned {Count} records", result.Count());

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAllAsync: {Message}", ex.Message);
            throw;
        }
    }
    public async Task<int> GetTotalCountAsync(
        string? searchTerm = null,
        int? companyId = null,
        int? departmentId = null,
        string? gender = null,
        string? status = null)
    {
        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(
            "sp_GetEmployeeCount",
            new
            {
                SearchTerm = searchTerm,
                CompanyId = companyId,
                DepartmentId = departmentId,
                Gender = gender,
                Status = status
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<Employee?> GetByCodeAsync(string employeeCode)
    {
        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Employee>(
            "sp_GetEmployeeByCode",
            new { EmployeeCode = employeeCode },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> ExistsAsync(string employeeCode)
    {
        using var connection = _context.CreateConnection();
        var result = await connection.ExecuteScalarAsync<int>(
            "sp_EmployeeExists",
            new { EmployeeCode = employeeCode },
            commandType: CommandType.StoredProcedure);
        return result > 0;
    }

    public async Task<IEnumerable<string>> GetExistingCodesAsync(IEnumerable<string> codes)
    {
        using var connection = _context.CreateConnection();
        var codesCsv = string.Join(",", codes);
        return await connection.QueryAsync<string>(
            "sp_GetExistingEmployeeCodes",
            new { EmployeeCodes = codesCsv },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<string>> GetExistingContactNumbersAsync(IEnumerable<string> contactNumbers)
    {
        using var connection = _context.CreateConnection();
        var contactsCsv = string.Join(",", contactNumbers);
        return await connection.QueryAsync<string>(
            "sp_GetExistingContactNumbers",
            new { ContactNumbers = contactsCsv },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<ExistingEmployeeInfo>> GetEmployeesByContactNumbersAsync(IEnumerable<string> contactNumbers)
    {
        using var connection = _context.CreateConnection();
        var contactsCsv = string.Join(",", contactNumbers);
        return await connection.QueryAsync<ExistingEmployeeInfo>(
            "sp_GetEmployeesByContactNumbers",
            new { ContactNumbers = contactsCsv },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> InsertAsync(Employee employee)
    {
        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(
            "sp_InsertEmployee",
            new
            {
                employee.EmployeeCode,
                employee.EmployeeName,
                employee.Gender,
                employee.CompanyId,
                employee.DepartmentId,
                employee.CategoryId,
                employee.Designation,
                employee.DOJ,
                employee.DOB,
                employee.EmployementType,
                employee.Status,
                employee.ContactNo,
                employee.Email,
                employee.Location,
                employee.AadhaarNumber,
                employee.ResidentialAddress,
                employee.PermanentAddress,
                employee.FatherName,
                employee.MotherName,
                employee.BloodGroup
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> UpdateAsync(Employee employee)
    {
        using var connection = _context.CreateConnection();
        return await connection.ExecuteAsync(
            "sp_UpdateEmployee",
            new
            {
                employee.EmployeeCode,
                employee.EmployeeName,
                employee.Gender,
                employee.CompanyId,
                employee.DepartmentId,
                employee.CategoryId,
                employee.Designation,
                employee.DOJ,
                employee.DOB,
                employee.EmployementType,
                employee.Status,
                employee.ContactNo,
                employee.Email,
                employee.Location,
                employee.AadhaarNumber,
                employee.ResidentialAddress,
                employee.PermanentAddress,
                employee.FatherName,
                employee.MotherName,
                employee.BloodGroup
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> DeleteAsync(string employeeCode)
    {
        using var connection = _context.CreateConnection();
        return await connection.ExecuteAsync(
            "sp_DeleteEmployee",
            new { EmployeeCode = employeeCode },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<(int EmployeeId, string EmployeeCode, string Operation)> UpsertByContactNoAsync(Employee employee)
    {
        using var connection = _context.CreateConnection();
        var result = await connection.QueryFirstAsync<dynamic>(
            "sp_UpsertEmployeeByContactNo",
            new
            {
                employee.EmployeeName,
                employee.Gender,
                employee.ContactNo,
                employee.CompanyId,
                employee.DepartmentId,
                employee.CategoryId,
                employee.AadhaarNumber,
                employee.ResidentialAddress,
                employee.PermanentAddress,
                employee.Email,
                employee.Location,
                employee.Designation,
                employee.DOJ,
                employee.DOB,
                employee.EmployementType,
                employee.Status,
                employee.FatherName,
                employee.MotherName,
                employee.BloodGroup
            },
            commandType: CommandType.StoredProcedure);

        return (result.EmployeeId, result.EmployeeCode, result.Operation);
    }

    public async Task<(int Inserted, int Updated)> UpsertBatchByContactNoAsync(IEnumerable<Employee> employees)
    {
        int inserted = 0;
        int updated = 0;

        foreach (var employee in employees)
        {
            try
            {
                var result = await UpsertByContactNoAsync(employee);
                if (result.Operation == "INSERT")
                    inserted++;
                else if (result.Operation == "UPDATE")
                    updated++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error upserting employee with contact {ContactNo}", employee.ContactNo);
            }
        }

        return (inserted, updated);
    }

    public async Task<IEnumerable<Employee>> GetForExportAsync(
        int? companyId = null,
        int? departmentId = null,
        string? status = null)
    {
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Employee>(
            "sp_GetEmployeesForExport",
            new
            {
                CompanyId = companyId,
                DepartmentId = departmentId,
                Status = status
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<CompanyDropdown>> GetCompaniesDropdownAsync()
    {
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<CompanyDropdown>(
            "sp_GetCompaniesDropdown",
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<DepartmentDropdown>> GetDepartmentsDropdownAsync(int? companyId = null)
    {
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<DepartmentDropdown>(
            "sp_GetDepartmentsDropdown",
            new { CompanyId = companyId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<CategoryDropdown>> GetCategoriesDropdownAsync()
    {
        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<CategoryDropdown>(
            "sp_GetCategoriesDropdown",
            commandType: CommandType.StoredProcedure);
    }
}