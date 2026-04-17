using CSMTutorial.Data.Repositories;
using CSMTutorial.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace CSMTutorial.Services;

public class ExcelService : IExcelService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILogger<ExcelService> _logger;

    public ExcelService(IEmployeeRepository employeeRepository, ILogger<ExcelService> logger)
    {
        _employeeRepository = employeeRepository;
        _logger = logger;
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public async Task<ExcelUploadResult> ParseExcelFileAsync(Stream fileStream, string fileName)
    {
        var result = new ExcelUploadResult();

        try
        {
            _logger.LogInformation("Starting to parse Excel file: {FileName}", fileName);

            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
            {
                result.Success = false;
                result.Message = "No worksheet found in the Excel file.";
                result.Errors.Add("The Excel file appears to be empty or corrupted.");
                return result;
            }

            var rowCount = worksheet.Dimension?.Rows ?? 0;
            var colCount = worksheet.Dimension?.Columns ?? 0;

            if (rowCount < 2)
            {
                result.Success = false;
                result.Message = "No data rows found in the Excel file.";
                result.Errors.Add("The Excel file must contain at least a header row and one data row.");
                return result;
            }

            _logger.LogInformation("Found {RowCount} rows and {ColCount} columns", rowCount, colCount);

            // Parse header row to map columns
            var columnMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int col = 1; col <= colCount; col++)
            {
                var headerValue = worksheet.Cells[1, col].Value?.ToString()?.Trim();
                if (!string.IsNullOrEmpty(headerValue))
                {
                    columnMap[headerValue] = col;
                    var normalized = headerValue.Replace(" ", "");
                    if (!columnMap.ContainsKey(normalized))
                        columnMap[normalized] = col;
                }
            }

            // Check for required columns
            var hasNameColumn = HasAnyColumn(columnMap, "StudentName", "Student Name", "EmployeeName", "Employee Name", "Name", "FirstName", "First Name");
            var hasGenderColumn = HasAnyColumn(columnMap, "Gender");
            var hasContactColumn = HasAnyColumn(columnMap, "ContactNumber", "Contact Number", "ContactNo", "Mobile", "MobileNumber", "Mobile Number", "PhoneNumber", "Phone Number");

            if (!hasNameColumn)
            {
                result.Success = false;
                result.Message = "Required column missing: Name (Student Name / Employee Name)";
                return result;
            }

            if (!hasGenderColumn)
            {
                result.Success = false;
                result.Message = "Required column missing: Gender";
                return result;
            }

            if (!hasContactColumn)
            {
                result.Success = false;
                result.Message = "Required column missing: Contact Number";
                return result;
            }

            // Parse data rows
            var records = new List<EmployeeImportDto>();
            for (int row = 2; row <= rowCount; row++)
            {
                var record = new EmployeeImportDto
                {
                    RowNumber = row,
                    EmployeeName = GetCellValueMultiKey(worksheet, row, columnMap,
                        "StudentName", "Student Name", "EmployeeName", "Employee Name", "Name", "FirstName", "First Name"),
                    EmployeeLastName = GetCellValueMultiKey(worksheet, row, columnMap,
                        "StudentLastName", "Student LastName", "EmployeeLastName", "Employee LastName", "LastName", "Last Name"),
                    Gender = GetCellValueMultiKey(worksheet, row, columnMap, "Gender"),
                    ContactNo = GetCellValueMultiKey(worksheet, row, columnMap,
                        "ContactNumber", "Contact Number", "ContactNo", "Mobile", "MobileNumber", "Mobile Number", "PhoneNumber", "Phone Number"),
                    AadhaarNumber = GetCellValueMultiKey(worksheet, row, columnMap,
                        "AdharNumber", "Adhar Number", "AadhaarNumber", "Aadhaar Number", "Aadhaar", "Adhar"),
                    Address = GetCellValueMultiKey(worksheet, row, columnMap,
                        "Address", "ResidenceAddress", "Residence Address", "ResidentialAddress", "Residential Address"),
                    Email = GetCellValueMultiKey(worksheet, row, columnMap, "Email", "eMail", "E-Mail"),
                    Location = GetCellValueMultiKey(worksheet, row, columnMap, "Location", "City", "Place"),
                    BloodGroup = GetCellValueMultiKey(worksheet, row, columnMap, "BloodGroup", "Blood Group"),
                    FatherName = GetCellValueMultiKey(worksheet, row, columnMap, "FatherName", "Father Name"),
                    MotherName = GetCellValueMultiKey(worksheet, row, columnMap, "MotherName", "Mother Name"),
                    Designation = GetCellValueMultiKey(worksheet, row, columnMap, "Designation", "Position"),
                    PermanentAddress = GetCellValueMultiKey(worksheet, row, columnMap, "PermanentAddress", "Permanent Address"),
                    EmploymentType = GetCellValueMultiKey(worksheet, row, columnMap, "EmploymentType", "Employment Type"),
                    Status = GetCellValueMultiKey(worksheet, row, columnMap, "Status"),
                };

                // Parse Age
                var ageStr = GetCellValueMultiKey(worksheet, row, columnMap, "Age");
                if (int.TryParse(ageStr, out var age))
                    record.Age = age;

                // Parse dates
                record.DOJ = ParseDateMultiKey(worksheet, row, columnMap, "DOJ", "DateOfJoining", "Date Of Joining");
                record.DOB = ParseDateMultiKey(worksheet, row, columnMap, "DOB", "DateOfBirth", "Date Of Birth");

                // Skip empty rows
                if (string.IsNullOrWhiteSpace(record.EmployeeName) && string.IsNullOrWhiteSpace(record.ContactNo))
                    continue;

                // Combine first and last name
                if (!string.IsNullOrWhiteSpace(record.EmployeeLastName))
                    record.EmployeeName = $"{record.EmployeeName} {record.EmployeeLastName}".Trim();

                // Use Address for ResidenceAddress
                if (string.IsNullOrWhiteSpace(record.ResidenceAddress) && !string.IsNullOrWhiteSpace(record.Address))
                    record.ResidenceAddress = record.Address;

                // Validate
                ValidateRecordForAutoCode(record);
                records.Add(record);
            }

            // Check for duplicates WITHIN the file (by Contact Number)
            var fileGroups = records
                .Where(r => !string.IsNullOrEmpty(r.ContactNo))
                .GroupBy(r => r.ContactNo)
                .Where(g => g.Count() > 1);

            foreach (var group in fileGroups)
            {
                foreach (var record in group.Skip(1))
                {
                    record.IsDuplicate = true;
                    record.DuplicateReason = $"Duplicate Contact Number '{record.ContactNo}' in file (Row {group.First().RowNumber} has same number)";
                    record.ValidationErrors.Add(record.DuplicateReason);
                }
            }

            // Check database for existing records (these will be UPDATED, not skipped)
            var validContactNumbers = records
                .Where(r => !string.IsNullOrEmpty(r.ContactNo) && !r.IsDuplicate && r.ValidationErrors.Count == 0)
                .Select(r => r.ContactNo)
                .Distinct();

            var existingEmployees = await _employeeRepository.GetEmployeesByContactNumbersAsync(validContactNumbers);
            var existingMap = existingEmployees.ToDictionary(e => e.ContactNo, e => e, StringComparer.OrdinalIgnoreCase);

            foreach (var record in records)
            {
                if (!record.IsDuplicate && existingMap.TryGetValue(record.ContactNo, out var existing))
                {
                    // Mark for UPDATE (not as duplicate/error)
                    record.IsUpdate = true;
                    record.ExistingEmployeeId = existing.EmployeeId;
                    record.ExistingEmployeeCode = existing.EmployeeCode;
                }
            }

            result.Success = true;
            result.Message = "File parsed successfully";
            result.Records = records;
            result.TotalRows = records.Count;
            result.ValidRows = records.Count(r => r.IsValid);
            result.InvalidRows = records.Count(r => !r.IsValid && !r.IsDuplicate);
            result.DuplicateRows = records.Count(r => r.IsDuplicate);
            result.NewRows = records.Count(r => r.IsValid && !r.IsUpdate);
            result.UpdateRows = records.Count(r => r.IsValid && r.IsUpdate);

            _logger.LogInformation(
                "Parse complete - Total: {Total}, New: {New}, Update: {Update}, Invalid: {Invalid}, Duplicates: {Duplicates}",
                result.TotalRows, result.NewRows, result.UpdateRows, result.InvalidRows, result.DuplicateRows);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Excel file: {FileName}", fileName);
            result.Success = false;
            result.Message = "Error parsing Excel file";
            result.Errors.Add(ex.Message);
            return result;
        }
    }
    // Helper method to check if any of the column names exist
    private bool HasAnyColumn(Dictionary<string, int> columnMap, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (columnMap.ContainsKey(key) || columnMap.ContainsKey(key.Replace(" ", "")))
                return true;
        }
        return false;
    }
    // Helper method to get cell value with multiple possible column names
    private string GetCellValueMultiKey(ExcelWorksheet worksheet, int row, Dictionary<string, int> columnMap, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (columnMap.TryGetValue(key, out var col) || columnMap.TryGetValue(key.Replace(" ", ""), out col))
            {
                var value = worksheet.Cells[row, col].Value?.ToString()?.Trim();
                if (!string.IsNullOrEmpty(value))
                    return value;
            }
        }
        return string.Empty;
    }

    // Helper method to parse date with multiple possible column names
    private DateTime? ParseDateMultiKey(ExcelWorksheet worksheet, int row, Dictionary<string, int> columnMap, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (columnMap.TryGetValue(key, out var col) || columnMap.TryGetValue(key.Replace(" ", ""), out col))
            {
                var cellValue = worksheet.Cells[row, col].Value;
                if (cellValue == null) continue;
                if (cellValue is DateTime dt) return dt;
                if (cellValue is double d) return DateTime.FromOADate(d);
                if (DateTime.TryParse(cellValue.ToString(), out var parsed)) return parsed;
            }
        }
        return null;
    }
    // Validation for auto-code scenario (no EmployeeCode required)
    private void ValidateRecordForAutoCode(EmployeeImportDto record)
    {
        if (string.IsNullOrWhiteSpace(record.EmployeeName))
            record.ValidationErrors.Add("Name is required");
        else if (record.EmployeeName.Length > 100)
            record.ValidationErrors.Add("Name must not exceed 100 characters");

        if (string.IsNullOrWhiteSpace(record.Gender))
            record.ValidationErrors.Add("Gender is required");
        else if (!new[] { "Male", "Female", "Other", "M", "F" }.Contains(record.Gender, StringComparer.OrdinalIgnoreCase))
            record.ValidationErrors.Add("Gender must be Male, Female, or Other");

        if (string.IsNullOrWhiteSpace(record.ContactNo))
            record.ValidationErrors.Add("Contact Number is required");
        else if (!System.Text.RegularExpressions.Regex.IsMatch(record.ContactNo, @"^\d{10}$"))
            record.ValidationErrors.Add("Contact Number must be exactly 10 digits");

        if (!string.IsNullOrEmpty(record.AadhaarNumber) &&
            !System.Text.RegularExpressions.Regex.IsMatch(record.AadhaarNumber, @"^\d{12}$"))
            record.ValidationErrors.Add("Aadhaar number must be exactly 12 digits");

        if (!string.IsNullOrEmpty(record.Email) && !IsValidEmail(record.Email))
            record.ValidationErrors.Add("Invalid email format");

        if (record.Age.HasValue && (record.Age < 16 || record.Age > 100))
            record.ValidationErrors.Add("Age must be between 16 and 100");
    }
    // Update the ImportEmployeesAsync method in ExcelService.cs

    public async Task<ImportResult> ImportEmployeesAsync(List<EmployeeImportDto> employees, int companyId, int departmentId)
    {
        var result = new ImportResult();

        try
        {
            var validRecords = employees.Where(e => e.IsValid && !e.IsDuplicate).ToList();

            if (!validRecords.Any())
            {
                result.Success = false;
                result.Message = "No valid records to import";
                return result;
            }

            _logger.LogInformation("Processing {Count} records (New: {New}, Update: {Update}) for Company: {CompanyId}, Department: {DepartmentId}",
                validRecords.Count,
                validRecords.Count(r => !r.IsUpdate),
                validRecords.Count(r => r.IsUpdate),
                companyId,
                departmentId);

            var employeeEntities = validRecords.Select(dto => new Employee
            {
                EmployeeName = dto.EmployeeName,
                Gender = NormalizeGender(dto.Gender),
                AadhaarNumber = dto.AadhaarNumber,
                ResidentialAddress = dto.Address ?? dto.ResidenceAddress,
                Age = dto.Age,
                Email = dto.Email,
                Location = dto.Location,
                ContactNo = dto.ContactNo,
                CompanyId = companyId,        // Use selected Company
                DepartmentId = departmentId,  // Use selected Department
                CategoryId = 1,
                Designation = dto.Designation,
                DOJ = dto.DOJ ?? DateTime.Now,
                DOB = dto.DOB,
                EmployementType = dto.EmploymentType ?? "Permanent",
                Status = dto.Status ?? "Active",
                FatherName = dto.FatherName,
                MotherName = dto.MotherName,
                BloodGroup = dto.BloodGroup,
                PermanentAddress = dto.PermanentAddress
            }).ToList();

            var (inserted, updated) = await _employeeRepository.UpsertBatchByContactNoAsync(employeeEntities);

            result.Success = true;
            result.InsertedCount = inserted;
            result.UpdatedCount = updated;
            result.SkippedCount = employees.Count - validRecords.Count;
            result.Message = $"Successfully processed {inserted + updated} employees. " +
                            $"New: {inserted} (codes auto-generated as CSM_XXXXX), " +
                            $"Updated: {updated}, " +
                            $"Skipped: {result.SkippedCount}";

            _logger.LogInformation("Import complete - Inserted: {Inserted}, Updated: {Updated}, Skipped: {Skipped}",
                inserted, updated, result.SkippedCount);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing employees");
            result.Success = false;
            result.Message = "Error importing employees";
            result.Errors.Add(ex.Message);
            return result;
        }
    }
    public byte[] GenerateSampleTemplate()
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Employee Import");

        // New simplified headers matching your data
        var headers = new[]
        {
        "Employee Name", "Employee LastName", "Gender", "AdharNumber",
        "Address", "Age", "Email", "Location", "Contact Number"
    };

        // Headers with styling
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cells[1, i + 1];
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;

            // Required columns = Green (Name, Gender, Contact Number)
            if (i == 0 || i == 2 || i == 8)
                cell.Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
            else
                cell.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
        }

        // Sample data (your provided data)
        var sampleRows = new object[,]
        {
        { "Rahul", "Patil", "Male", "123456789012", "Street 12, Area 3", 20, "rahul1@gmail.com", "Wardha", "9876543210" },
        { "Sneha", "Sharma", "Female", "234567890123", "Street 5, Area 1", 18, "sneha2@gmail.com", "Nagpur", "9123456780" },
        { "Amit", "Verma", "Male", "345678901234", "Street 22, Area 4", 22, "amit3@gmail.com", "Pune", "9988776655" }
        };

        for (int row = 0; row < sampleRows.GetLength(0); row++)
        {
            for (int col = 0; col < sampleRows.GetLength(1); col++)
            {
                worksheet.Cells[row + 2, col + 1].Value = sampleRows[row, col];
            }
        }

        // Instructions sheet
        var instructions = package.Workbook.Worksheets.Add("Instructions");
        instructions.Cells["A1"].Value = "Employee Import Template";
        instructions.Cells["A1"].Style.Font.Bold = true;
        instructions.Cells["A1"].Style.Font.Size = 16;

        instructions.Cells["A3"].Value = "Required Fields (Green):";
        instructions.Cells["A3"].Style.Font.Bold = true;
        instructions.Cells["A4"].Value = "• Employee Name - First name of employee";
        instructions.Cells["A5"].Value = "• Gender - Male / Female";
        instructions.Cells["A6"].Value = "• Contact Number - 10 digits (used for duplicate check)";

        instructions.Cells["A8"].Value = "Auto-Generated:";
        instructions.Cells["A8"].Style.Font.Bold = true;
        instructions.Cells["A9"].Value = "• Employee Code - Auto-generated as CSM_00001, CSM_00002, etc.";

        instructions.Cells["A11"].Value = "Important Notes:";
        instructions.Cells["A11"].Style.Font.Bold = true;
        instructions.Cells["A12"].Value = "• Contact Number must be unique - duplicates will be skipped";
        instructions.Cells["A13"].Value = "• Aadhaar Number should be 12 digits";
        instructions.Cells["A14"].Value = "• Age must be between 16 and 100";

        worksheet.Cells.AutoFitColumns();
        instructions.Cells.AutoFitColumns();

        return package.GetAsByteArray();
    }

    public byte[] ExportEmployeesToExcel(IEnumerable<Employee> employees)
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Employees");

        var headers = new[]
        {
            "Employee Code", "Employee Name", "Gender", "Designation",
            "Department", "Company", "Location", "Contact No", "Email",
            "Date of Joining", "Date of Birth", "Status", "Employment Type",
            "Father Name", "Mother Name", "Blood Group"
        };

        // Headers
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cells[1, i + 1];
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
        }

        // Data
        int row = 2;
        foreach (var emp in employees)
        {
            worksheet.Cells[row, 1].Value = emp.EmployeeCode;
            worksheet.Cells[row, 2].Value = emp.EmployeeName;
            worksheet.Cells[row, 3].Value = emp.Gender;
            worksheet.Cells[row, 4].Value = emp.Designation;
            worksheet.Cells[row, 5].Value = emp.DepartmentName;
            worksheet.Cells[row, 6].Value = emp.CompanyName;
            worksheet.Cells[row, 7].Value = emp.Location;
            worksheet.Cells[row, 8].Value = emp.ContactNo;
            worksheet.Cells[row, 9].Value = emp.Email;
            worksheet.Cells[row, 10].Value = emp.DOJ?.ToString("yyyy-MM-dd");
            worksheet.Cells[row, 11].Value = emp.DOB?.ToString("yyyy-MM-dd");
            worksheet.Cells[row, 12].Value = emp.Status;
            worksheet.Cells[row, 13].Value = emp.EmployementType;
            worksheet.Cells[row, 14].Value = emp.FatherName;
            worksheet.Cells[row, 15].Value = emp.MotherName;
            worksheet.Cells[row, 16].Value = emp.BloodGroup;
            row++;
        }

        worksheet.Cells.AutoFitColumns();

        return package.GetAsByteArray();
    }

    private string GetCellValue(ExcelWorksheet worksheet, int row, Dictionary<string, int> columnMap, string columnName)
    {
        if (!columnMap.TryGetValue(columnName, out var col))
            return string.Empty;

        return worksheet.Cells[row, col].Value?.ToString()?.Trim() ?? string.Empty;
    }

    private DateTime? ParseDate(ExcelWorksheet worksheet, int row, Dictionary<string, int> columnMap, string columnName)
    {
        if (!columnMap.TryGetValue(columnName, out var col))
            return null;

        var cellValue = worksheet.Cells[row, col].Value;

        if (cellValue == null)
            return null;

        if (cellValue is DateTime dt)
            return dt;

        if (cellValue is double d)
            return DateTime.FromOADate(d);

        if (DateTime.TryParse(cellValue.ToString(), out var parsed))
            return parsed;

        return null;
    }

    private bool ParseBooleanValue(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        return value.ToLower() switch
        {
            "yes" => true,
            "true" => true,
            "1" => true,
            _ => false
        };
    }

    private string NormalizeGender(string gender)
    {
        return gender?.ToLower() switch
        {
            "m" => "Male",
            "f" => "Female",
            "male" => "Male",
            "female" => "Female",
            "other" => "Other",
            _ => gender ?? "Other"
        };
    }

    private void ValidateRecord(EmployeeImportDto record)
    {
        if (string.IsNullOrWhiteSpace(record.EmployeeCode))
            record.ValidationErrors.Add("EmployeeCode is required");
        else if (record.EmployeeCode.Length > 50)
            record.ValidationErrors.Add("EmployeeCode must not exceed 50 characters");

        if (string.IsNullOrWhiteSpace(record.EmployeeName))
            record.ValidationErrors.Add("EmployeeName is required");
        else if (record.EmployeeName.Length > 50)
            record.ValidationErrors.Add("EmployeeName must not exceed 50 characters");

        if (string.IsNullOrWhiteSpace(record.Gender))
            record.ValidationErrors.Add("Gender is required");
        else if (!new[] { "Male", "Female", "Other", "M", "F" }.Contains(record.Gender, StringComparer.OrdinalIgnoreCase))
            record.ValidationErrors.Add("Gender must be Male, Female, or Other");

        if (!string.IsNullOrEmpty(record.Email) && !IsValidEmail(record.Email))
            record.ValidationErrors.Add("Invalid email format");

        if (record.DOJ.HasValue && record.DOJ.Value > DateTime.Now.AddDays(30))
            record.ValidationErrors.Add("Date of Joining cannot be more than 30 days in the future");

        if (record.DOB.HasValue && record.DOB.Value > DateTime.Now.AddYears(-16))
            record.ValidationErrors.Add("Employee must be at least 16 years old");
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
    // Add this method to your existing ExcelService.cs

    public byte[] ExportAttendanceToExcel(IEnumerable<DailyAttendanceRecord> records, string employeeName, string monthYear)
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Attendance");

        // Title
        worksheet.Cells["A1"].Value = $"Attendance Report - {employeeName}";
        worksheet.Cells["A1"].Style.Font.Bold = true;
        worksheet.Cells["A1"].Style.Font.Size = 16;
        worksheet.Cells["A1:L1"].Merge = true;

        worksheet.Cells["A2"].Value = $"Month: {monthYear}";
        worksheet.Cells["A2"].Style.Font.Size = 12;
        worksheet.Cells["A2:L2"].Merge = true;

        // Headers
        var headers = new[]
        {
        "Date", "Day", "Shift", "In Time", "Out Time",
        "Duration", "Late By", "Early By", "Overtime",
        "Status", "Punch Records", "Remarks"
    };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cells[4, i + 1];
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(68, 114, 196));
            cell.Style.Font.Color.SetColor(Color.White);
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        }

        // Data
        int row = 5;
        int totalPresent = 0, totalAbsent = 0, totalLate = 0;
        double totalDuration = 0, totalOT = 0;

        foreach (var record in records)
        {
            worksheet.Cells[row, 1].Value = record.AttendanceDate.ToString("dd-MMM-yyyy");
            worksheet.Cells[row, 2].Value = record.AttendanceDate.ToString("dddd");
            worksheet.Cells[row, 3].Value = record.ShiftName ?? "-";
            worksheet.Cells[row, 4].Value = record.InTime ?? "-";
            worksheet.Cells[row, 5].Value = record.OutTime ?? "-";
            worksheet.Cells[row, 6].Value = record.FormattedDuration;
            worksheet.Cells[row, 7].Value = record.LateBy > 0 ? record.FormattedLateBy : "-";
            worksheet.Cells[row, 8].Value = record.EarlyBy > 0 ? record.FormattedEarlyBy : "-";
            worksheet.Cells[row, 9].Value = record.OverTime > 0 ? record.FormattedOverTime : "-";
            worksheet.Cells[row, 10].Value = record.StatusDisplay;
            worksheet.Cells[row, 11].Value = record.PunchRecords;
            worksheet.Cells[row, 12].Value = record.Remarks ?? "-";

            // Row coloring
            Color bgColor = Color.White;
            if (record.Absent > 0) bgColor = Color.FromArgb(255, 230, 230);
            else if (record.WeeklyOff == 1) bgColor = Color.FromArgb(230, 230, 230);
            else if (record.Holiday == 1) bgColor = Color.FromArgb(230, 245, 255);
            else if (record.IsOnLeave == 1) bgColor = Color.FromArgb(255, 245, 230);
            else if (record.LateBy > 0) bgColor = Color.FromArgb(255, 255, 230);

            for (int c = 1; c <= 12; c++)
            {
                worksheet.Cells[row, c].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, c].Style.Fill.BackgroundColor.SetColor(bgColor);
                worksheet.Cells[row, c].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
            }

            // Totals
            if (record.Present > 0) totalPresent++;
            if (record.Absent > 0) totalAbsent++;
            if (record.LateBy > 0) totalLate++;
            totalDuration += record.Duration;
            totalOT += record.OverTime;

            row++;
        }

        // Summary Section
        row += 2;
        worksheet.Cells[row, 1].Value = "SUMMARY";
        worksheet.Cells[row, 1].Style.Font.Bold = true;
        worksheet.Cells[row, 1].Style.Font.Size = 14;
        worksheet.Cells[$"A{row}:D{row}"].Merge = true;

        row++;
        var summaryData = new (string Label, string Value)[]
        {
        ("Total Days", records.Count().ToString()),
        ("Present", totalPresent.ToString()),
        ("Absent", totalAbsent.ToString()),
        ("Late Days", totalLate.ToString()),
        ("Total Working Hours", TimeSpan.FromMinutes(totalDuration).ToString(@"hh\:mm")),
        ("Total Overtime", TimeSpan.FromMinutes(totalOT).ToString(@"hh\:mm")),
        };

        foreach (var item in summaryData)
        {
            worksheet.Cells[row, 1].Value = item.Label;
            worksheet.Cells[row, 1].Style.Font.Bold = true;
            worksheet.Cells[row, 2].Value = item.Value;
            row++;
        }

        // Legend
        row += 2;
        worksheet.Cells[row, 1].Value = "LEGEND:";
        worksheet.Cells[row, 1].Style.Font.Bold = true;
        row++;

        var legends = new (string Label, Color BgColor)[]
        {
        ("Present", Color.White),
        ("Absent", Color.FromArgb(255, 230, 230)),
        ("Weekly Off", Color.FromArgb(230, 230, 230)),
        ("Holiday", Color.FromArgb(230, 245, 255)),
        ("Leave", Color.FromArgb(255, 245, 230)),
        ("Late", Color.FromArgb(255, 255, 230))
        };

        foreach (var legend in legends)
        {
            worksheet.Cells[row, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(legend.BgColor);
            worksheet.Cells[row, 1].Value = "   ";
            worksheet.Cells[row, 2].Value = legend.Label;
            row++;
        }

        worksheet.Cells.AutoFitColumns();
        worksheet.Column(11).Width = 40; // Punch Records column wider

        return package.GetAsByteArray();
    }
}