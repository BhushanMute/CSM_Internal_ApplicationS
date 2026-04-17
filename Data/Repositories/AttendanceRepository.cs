using CSMTutorial.Models;
using Dapper;
using System.Data;

namespace CSMTutorial.Data.Repositories;

public class AttendanceRepository : IAttendanceRepository
{
    private readonly DapperContext _context;
    private readonly ILogger<AttendanceRepository> _logger;

    public AttendanceRepository(DapperContext context, ILogger<AttendanceRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<DailyAttendanceRecord>> GetDailyAttendanceAsync(
        DateTime attendanceDate, int? companyId = null, int? departmentId = null,
        string? status = null, string? searchTerm = null, int pageNumber = 1, int pageSize = 50)
    {
        try
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<DailyAttendanceRecord>(
                "sp_GetDailyAttendanceReport",
                new { AttendanceDate = attendanceDate, CompanyId = companyId, DepartmentId = departmentId, Status = status, SearchTerm = searchTerm, PageNumber = pageNumber, PageSize = pageSize },
                commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex) { _logger.LogError(ex, "Error getting daily attendance"); throw; }
    }

    public async Task<int> GetDailyAttendanceCountAsync(
        DateTime attendanceDate, int? companyId = null, int? departmentId = null,
        string? status = null, string? searchTerm = null)
    {
        try
        {
            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(
                "sp_GetDailyAttendanceCount",
                new { AttendanceDate = attendanceDate, CompanyId = companyId, DepartmentId = departmentId, Status = status, SearchTerm = searchTerm },
                commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex) { _logger.LogError(ex, "Error getting daily attendance count"); throw; }
    }

    public async Task<AttendanceSummary> GetAttendanceSummaryAsync(
        DateTime attendanceDate, int? companyId = null, int? departmentId = null)
    {
        try
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<AttendanceSummary>(
                "sp_GetAttendanceSummary",
                new { AttendanceDate = attendanceDate, CompanyId = companyId, DepartmentId = departmentId },
                commandType: CommandType.StoredProcedure) ?? new AttendanceSummary();
        }
        catch (Exception ex) { _logger.LogError(ex, "Error getting attendance summary"); throw; }
    }

    public async Task<IEnumerable<DeviceLogRecord>> GetDeviceLogsAsync(
        int month, int year, DateTime? attendanceDate = null, int? employeeId = null,
        int? companyId = null, int? departmentId = null)
    {
        try
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<DeviceLogRecord>(
                "sp_GetDeviceLogs",
                new { Month = month, Year = year, AttendanceDate = attendanceDate, EmployeeId = employeeId, CompanyId = companyId, DepartmentId = departmentId },
                commandType: CommandType.StoredProcedure, commandTimeout: 120);
        }
        catch (Exception ex) { _logger.LogError(ex, "Error getting device logs"); throw; }
    }

    public async Task<IEnumerable<DeviceLogTableInfo>> GetAvailableDeviceLogTablesAsync()
    {
        try
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<DeviceLogTableInfo>(
                "sp_GetAvailableDeviceLogTables",
                commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex) { _logger.LogError(ex, "Error getting available log tables"); throw; }
    }

    public async Task<IEnumerable<PunchDetail>> GetEmployeePunchDetailsAsync(int employeeId, DateTime punchDate)
    {
        try
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<PunchDetail>(
                "sp_GetEmployeePunchDetails",
                new { EmployeeId = employeeId, PunchDate = punchDate },
                commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex) { _logger.LogError(ex, "Error getting punch details"); throw; }
    }

    // ===== NEW METHODS =====

    public async Task<IEnumerable<DailyAttendanceRecord>> GetEmployeeMonthlyAttendanceAsync(
        int employeeId, int month, int year)
    {
        try
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<DailyAttendanceRecord>(
                "sp_GetEmployeeMonthlyAttendance",
                new { EmployeeId = employeeId, Month = month, Year = year },
                commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting monthly attendance for employee {Id}", employeeId);
            throw;
        }
    }

    public async Task<EmployeeMonthlyAttendanceSummary> GetEmployeeMonthlyAttendanceSummaryAsync(
        int employeeId, int month, int year)
    {
        try
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<EmployeeMonthlyAttendanceSummary>(
                "sp_GetEmployeeMonthlyAttendanceSummary",
                new { EmployeeId = employeeId, Month = month, Year = year },
                commandType: CommandType.StoredProcedure) ?? new EmployeeMonthlyAttendanceSummary();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting monthly summary for employee {Id}", employeeId);
            throw;
        }
    }

    public async Task<IEnumerable<EmployeeListItem>> GetEmployeesByCompanyDepartmentAsync(
        int? companyId = null, int? departmentId = null, string? searchTerm = null,
        int pageNumber = 1, int pageSize = 50)
    {
        try
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<EmployeeListItem>(
                "sp_GetEmployeesByCompanyDepartment",
                new { CompanyId = companyId, DepartmentId = departmentId, SearchTerm = searchTerm, PageNumber = pageNumber, PageSize = pageSize },
                commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employees by company/department");
            throw;
        }
    }

    public async Task<int> GetEmployeesCountByCompanyDepartmentAsync(
        int? companyId = null, int? departmentId = null, string? searchTerm = null)
    {
        try
        {
            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(
                "sp_GetEmployeesCountByCompanyDepartment",
                new { CompanyId = companyId, DepartmentId = departmentId, SearchTerm = searchTerm },
                commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employees count");
            throw;
        }
    }

    public async Task<IEnumerable<PunchDetail>> GetEmployeePunchDetailsByMonthAsync(
        int employeeId, int month, int year, DateTime? punchDate = null)
    {
        try
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<PunchDetail>(
                "sp_GetEmployeePunchDetailsByMonth",
                new { EmployeeId = employeeId, Month = month, Year = year, PunchDate = punchDate },
                commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting punch details by month");
            throw;
        }
    }

    public async Task<IEnumerable<AttendanceMonthInfo>> GetAvailableAttendanceMonthsAsync(int employeeId)
    {
        try
        {
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<AttendanceMonthInfo>(
                "sp_GetAvailableAttendanceMonths",
                new { EmployeeId = employeeId },
                commandType: CommandType.StoredProcedure);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available months for employee {Id}", employeeId);
            throw;
        }
    }
}