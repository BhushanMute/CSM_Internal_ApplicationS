using CSMTutorial.Models;

namespace CSMTutorial.Data.Repositories;

public interface IAttendanceRepository
{
    // Daily Attendance Report
    Task<IEnumerable<DailyAttendanceRecord>> GetDailyAttendanceAsync(
        DateTime attendanceDate,
        int? companyId = null,
        int? departmentId = null,
        string? status = null,
        string? searchTerm = null,
        int pageNumber = 1,
        int pageSize = 50);

    Task<int> GetDailyAttendanceCountAsync(
        DateTime attendanceDate,
        int? companyId = null,
        int? departmentId = null,
        string? status = null,
        string? searchTerm = null);

    Task<AttendanceSummary> GetAttendanceSummaryAsync(
        DateTime attendanceDate,
        int? companyId = null,
        int? departmentId = null);

    // Device Logs
    Task<IEnumerable<DeviceLogRecord>> GetDeviceLogsAsync(
        int month, int year,
        DateTime? attendanceDate = null,
        int? employeeId = null,
        int? companyId = null,
        int? departmentId = null);

    Task<IEnumerable<DeviceLogTableInfo>> GetAvailableDeviceLogTablesAsync();

    Task<IEnumerable<PunchDetail>> GetEmployeePunchDetailsAsync(
        int employeeId, DateTime punchDate);

    // === NEW: Employee Monthly Attendance ===
    Task<IEnumerable<DailyAttendanceRecord>> GetEmployeeMonthlyAttendanceAsync(
        int employeeId, int month, int year);

    Task<EmployeeMonthlyAttendanceSummary> GetEmployeeMonthlyAttendanceSummaryAsync(
        int employeeId, int month, int year);

    Task<IEnumerable<EmployeeListItem>> GetEmployeesByCompanyDepartmentAsync(
        int? companyId = null,
        int? departmentId = null,
        string? searchTerm = null,
        int pageNumber = 1,
        int pageSize = 50);

    Task<int> GetEmployeesCountByCompanyDepartmentAsync(
        int? companyId = null,
        int? departmentId = null,
        string? searchTerm = null);

    Task<IEnumerable<PunchDetail>> GetEmployeePunchDetailsByMonthAsync(
        int employeeId, int month, int year, DateTime? punchDate = null);

    Task<IEnumerable<AttendanceMonthInfo>> GetAvailableAttendanceMonthsAsync(int employeeId);
}