namespace CSMTutorial.Models
{
    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? TokenExpiry { get; set; }
        public UserDto? User { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
