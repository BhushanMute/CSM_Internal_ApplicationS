namespace CSMTutorial.Models
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string LoginName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Role { get; set; }
        public bool IsAdmin { get; set; }
        public string? FullName { get; set; }
        public DateTime? LastLoginDate { get; set; }  // Added this property
        public DateTime? CreatedDate { get; set; }    // Added this property
    }
}
