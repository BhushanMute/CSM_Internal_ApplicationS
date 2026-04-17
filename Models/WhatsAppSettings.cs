 
 
 
namespace CSMTutorial.Models
{
    public class WhatsAppSettings
    {
        public int Id { get; set; }
        public bool IsEnabled { get; set; }
        public string APIProvider { get; set; } = "Gupshup";
        public string SenderNumber { get; set; } = "";
        public string APIEndpoint { get; set; } = "";
        public string APIKey { get; set; } = "";
        public string APISecret { get; set; } = "";
        public bool SendCheckInAlert { get; set; } = true;
        public bool SendCheckOutAlert { get; set; } = true;
        public string CheckInMessageFormat { get; set; } = "Dear Parent, {{StudentName}} has checked IN at {{BranchName}} on {{Date}} at {{Time}}.";
        public string CheckOutMessageFormat { get; set; } = "Dear Parent, {{StudentName}} has checked OUT from {{BranchName}} on {{Date}} at {{Time}}.";
        public bool IsTestMode { get; set; }
        public string TestMobileNumber { get; set; } = "";
        public string DefaultCountryCode { get; set; } = "91";
    }
}