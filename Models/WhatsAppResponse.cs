namespace CSMTutorial.Models
{
    public class WhatsAppResponse
    {
        public bool Success { get; set; }
        public string MessageId { get; set; }
        public string ErrorMessage { get; set; }
        public string RawResponse { get; set; }
    }
}
