namespace CSMTutorial.Models
{
    public class AttendanceMonthInfo
    {
        public int AttMonth { get; set; }
        public int AttYear { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int RecordCount { get; set; }
        public string DisplayName => $"{MonthName} {AttYear} ({RecordCount} days)";
    }
}
