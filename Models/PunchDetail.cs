namespace CSMTutorial.Models
{
    public class PunchDetail
    {
        public int DeviceLogId { get; set; }
        public DateTime LogDate { get; set; }
        public string? Direction { get; set; }
        public string? AttDirection { get; set; }
        public int? DeviceId { get; set; }
        public string? DeviceName { get; set; }
        public string? Longitude { get; set; }
        public string? Latitude { get; set; }
        public string? LocationAddress { get; set; }
        public double? BodyTemperature { get; set; }
        public int? IsMaskOn { get; set; }

        public string FormattedTime => LogDate.ToString("hh:mm:ss tt");
    }
}
