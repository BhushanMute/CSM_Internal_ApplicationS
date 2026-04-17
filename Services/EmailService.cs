using CSMTutorial.Models;
 
using CSMTutorial.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CSMTutorial.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly AppSettings _appSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailSettings> emailSettings,
        IOptions<AppSettings> appSettings,
        ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _appSettings = appSettings.Value;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress(toEmail, toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();

            await client.ConnectAsync(
                _emailSettings.SmtpServer,
                _emailSettings.SmtpPort,
                _emailSettings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

            await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            return false;
        }
    }

    public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string username, string resetLink)
    {
        var subject = "Password Reset Request - Blazor Excel Demo";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .button {{ display: inline-block; background: #667eea; color: white; padding: 15px 30px; text-decoration: none; border-radius: 8px; margin: 20px 0; }}
        .button:hover {{ background: #5a6fd6; }}
        .footer {{ text-align: center; margin-top: 20px; color: #888; font-size: 12px; }}
        .warning {{ background: #fff3cd; border: 1px solid #ffc107; padding: 15px; border-radius: 8px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔐 Password Reset</h1>
        </div>
        <div class='content'>
            <p>Hello <strong>{username}</strong>,</p>
            <p>We received a request to reset your password for your Blazor Excel Demo account.</p>
            <p>Click the button below to reset your password:</p>
            <p style='text-align: center;'>
                <a href='{resetLink}' class='button'>Reset Password</a>
            </p>
            <div class='warning'>
                <strong>⚠️ Important:</strong>
                <ul>
                    <li>This link will expire in {_appSettings.PasswordResetTokenExpiryHours} hours</li>
                    <li>If you didn't request this, please ignore this email</li>
                    <li>Your password won't change until you create a new one</li>
                </ul>
            </div>
            <p>If the button doesn't work, copy and paste this link into your browser:</p>
            <p style='word-break: break-all; background: #eee; padding: 10px; border-radius: 5px;'>{resetLink}</p>
        </div>
        <div class='footer'>
            <p>This email was sent by Blazor Excel Demo</p>
            <p>© {DateTime.Now.Year} All rights reserved</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task<bool> SendWelcomeEmailAsync(string toEmail, string username)
    {
        var subject = "Welcome to Blazor Excel Demo!";
        var loginLink = $"{_appSettings.BaseUrl}/login";

        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #28a745 0%, #20c997 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .button {{ display: inline-block; background: #28a745; color: white; padding: 15px 30px; text-decoration: none; border-radius: 8px; margin: 20px 0; }}
        .features {{ background: white; padding: 20px; border-radius: 8px; margin: 20px 0; }}
        .feature-item {{ padding: 10px 0; border-bottom: 1px solid #eee; }}
        .footer {{ text-align: center; margin-top: 20px; color: #888; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 Welcome!</h1>
        </div>
        <div class='content'>
            <p>Hello <strong>{username}</strong>,</p>
            <p>Your account has been successfully created! We're excited to have you on board.</p>
            
            <div class='features'>
                <h3>What you can do:</h3>
                <div class='feature-item'>📊 Upload Excel files for data import</div>
                <div class='feature-item'>👥 Manage employee records</div>
                <div class='feature-item'>📈 View dashboard analytics</div>
            </div>
            
            <p style='text-align: center;'>
                <a href='{loginLink}' class='button'>Login to Your Account</a>
            </p>
        </div>
        <div class='footer'>
            <p>Thank you for joining Blazor Excel Demo!</p>
            <p>© {DateTime.Now.Year} All rights reserved</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task<bool> SendEmailVerificationAsync(string toEmail, string username, string verificationLink)
    {
        var subject = "Verify Your Email - Blazor Excel Demo";

        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #17a2b8 0%, #6f42c1 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .button {{ display: inline-block; background: #17a2b8; color: white; padding: 15px 30px; text-decoration: none; border-radius: 8px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 20px; color: #888; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>✉️ Verify Your Email</h1>
        </div>
        <div class='content'>
            <p>Hello <strong>{username}</strong>,</p>
            <p>Thank you for registering! Please verify your email address by clicking the button below:</p>
            <p style='text-align: center;'>
                <a href='{verificationLink}' class='button'>Verify Email</a>
            </p>
            <p>If you didn't create an account, you can safely ignore this email.</p>
        </div>
        <div class='footer'>
            <p>© {DateTime.Now.Year} Blazor Excel Demo</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(toEmail, subject, htmlBody);
    }

    public async Task<bool> SendPasswordChangedNotificationAsync(string toEmail, string username)
    {
        var subject = "Password Changed Successfully - Blazor Excel Demo";

        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #28a745 0%, #20c997 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .warning {{ background: #fff3cd; border: 1px solid #ffc107; padding: 15px; border-radius: 8px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 20px; color: #888; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>✅ Password Changed</h1>
        </div>
        <div class='content'>
            <p>Hello <strong>{username}</strong>,</p>
            <p>Your password has been successfully changed on {DateTime.Now:MMMM dd, yyyy} at {DateTime.Now:hh:mm tt}.</p>
            <div class='warning'>
                <strong>⚠️ Wasn't you?</strong>
                <p>If you didn't make this change, please contact our support team immediately and secure your account.</p>
            </div>
        </div>
        <div class='footer'>
            <p>© {DateTime.Now.Year} Blazor Excel Demo</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(toEmail, subject, htmlBody);
    }
}