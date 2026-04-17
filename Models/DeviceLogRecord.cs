namespace CSMTutorial.Models
{
    public class DeviceLogRecord
    {
        public int DeviceLogId { get; set; }
        public DateTime? DownloadDate { get; set; }
        public int? DeviceId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime LogDate { get; set; }
        public string? Direction { get; set; }
        public string? AttDirection { get; set; }
        public string? C1 { get; set; }
        public string? C2 { get; set; }
        public string? C3 { get; set; }
        public string? C4 { get; set; }
        public string? C5 { get; set; }
        public string? C6 { get; set; }
        public string? C7 { get; set; }
        public string? WorkCode { get; set; }
        public string? Longitude { get; set; }
        public string? Latitude { get; set; }
        public string? LocationAddress { get; set; }
        public double? BodyTemperature { get; set; }
        public int? IsMaskOn { get; set; }

        // Joined fields
        public int? EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public string? Gender { get; set; }
        public int? CompanyId { get; set; }
        public int? DepartmentId { get; set; }
        public string? CompanyName { get; set; }
        public string? DepartmentName { get; set; }
        public string? DeviceName { get; set; }

        // Computed
        public string FormattedTime => LogDate.ToString("hh:mm:ss tt");
        public string DirectionDisplay => Direction?.ToUpper() switch
        {
            "0" or "IN" => "IN",
            "1" or "OUT" => "OUT",
            _ => Direction ?? "Unknown"
        };
    }

}
