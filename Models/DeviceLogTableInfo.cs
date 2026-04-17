namespace CSMTutorial.Models
{
    public class DeviceLogTableInfo
    {
        public string TableName { get; set; } = string.Empty;
        public int LogYear { get; set; }
        public int LogMonth { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public long RecordCount { get; set; }

        public string DisplayName => $"{MonthName} {LogYear} ({RecordCount:N0} records)";
    }
}
