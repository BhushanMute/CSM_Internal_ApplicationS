using CSMTutorial.Models;

namespace CSMTutorial.Services;

public interface IExcelService
{
    Task<ExcelUploadResult> ParseExcelFileAsync(Stream fileStream, string fileName);
    Task<ImportResult> ImportEmployeesAsync(List<EmployeeImportDto> employees, int companyId, int departmentId);
    byte[] GenerateSampleTemplate();
    byte[] ExportEmployeesToExcel(IEnumerable<Employee> employees);
    byte[] ExportAttendanceToExcel(IEnumerable<DailyAttendanceRecord> records, string employeeName, string monthYear);
}