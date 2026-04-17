namespace CSMTutorial.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody);
        Task<bool> SendPasswordResetEmailAsync(string toEmail, string username, string resetLink);
        Task<bool> SendWelcomeEmailAsync(string toEmail, string username);
        Task<bool> SendEmailVerificationAsync(string toEmail, string username, string verificationLink);
        Task<bool> SendPasswordChangedNotificationAsync(string toEmail, string username);
    }
}
