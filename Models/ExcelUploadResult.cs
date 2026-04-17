namespace CSMTutorial.Models;

public class ExcelUploadResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
    public List<EmployeeImportDto> Records { get; set; } = new();

    public int TotalRows { get; set; }
    public int ValidRows { get; set; }
    public int InvalidRows { get; set; }
    public int DuplicateRows { get; set; }  // Duplicates within the file
    public int NewRows { get; set; }        // New records to insert
    public int UpdateRows { get; set; }     // Existing records to update
}