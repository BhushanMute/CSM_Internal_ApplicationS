// Models/WhatsAppConfig.cs
namespace CSMTutorial.Models
{
    public class WhatsAppConfig
    {
        public int ConfigId { get; set; }
        public bool IsEnabled { get; set; }
        public string InstituteName { get; set; } = "";
        public string ContactNumber { get; set; } = "";
        public string DefaultCountryCode { get; set; } = "91";
        public int RefreshInterval { get; set; } = 60;

        public bool SendCheckInAlert { get; set; }
        public bool SendCheckOutAlert { get; set; }
        public bool SendAbsentAlert { get; set; }
        public bool SendLateAlert { get; set; }

        public TimeSpan AbsentAlertTime { get; set; } = new TimeSpan(10, 0, 0);
        public int LateThresholdMinutes { get; set; } = 15;

        public string CheckInMessage { get; set; } = "Dear Parent, {{StudentName}} has checked IN at {{Time}}.";
        public string CheckOutMessage { get; set; } = "Dear Parent, {{StudentName}} has checked OUT at {{Time}}.";
        public string AbsentMessage { get; set; } = "Dear Parent, {{StudentName}} was absent on {{Date}}.";
        public string LateArrivalMessage { get; set; } = "Dear Parent, {{StudentName}} arrived late at {{Time}}.";
        
        public string APIEndpoint { get; set; }
        public string APIKey { get; set; }
        public string APISecret { get; set; }
        public string AuthToken { get; set; }
        public string SenderNumber { get; set; }
        
    }
}