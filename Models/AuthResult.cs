using CSMTutorial.Models;

namespace CSMTutorial.Models
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public User? User { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
