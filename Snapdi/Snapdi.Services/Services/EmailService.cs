using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Snapdi.Services.Interfaces;
using Snapdi.Services.Models;
using System.Net;
using System.Net.Mail;

namespace Snapdi.Services.Services
{
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

        public async Task<bool> SendEmailVerificationAsync(string toEmail, string userName, string verificationToken)
        {
            try
            {
                var subject = "Verify Your Email Address - Snapdi";
                var verificationUrl = $"{_appSettings.BaseUrl}/api/auth/verify-email?token={verificationToken}";

                var body = $@"
                    <html>
                    <body>
                        <h2>Welcome to Snapdi, {userName}!</h2>
                        <p>Thank you for registering with Snapdi. To complete your registration, please verify your email address by clicking the link below:</p>
                        <p><a href='{verificationUrl}' style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 4px;'>Verify Email Address</a></p>
                        <p>Or copy and paste this link into your browser:</p>
                        <p>{verificationUrl}</p>
                        <p>This verification link will expire in 24 hours.</p>
                        <p>If you didn't create an account with Snapdi, please ignore this email.</p>
                        <br>
                        <p>Best regards,<br>The Snapdi Team</p>
                    </body>
                    </html>";

                return await SendEmailAsync(toEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email verification to {Email}", toEmail);
                return false;
            }
        }

        public async Task<bool> SendPasswordResetAsync(string toEmail, string userName, string resetToken)
        {
            try
            {
                var subject = "Reset Your Password - Snapdi";
                var resetUrl = $"{_appSettings.BaseUrl}/reset-password?token={resetToken}";

                var body = $@"
                    <html>
                    <body>
                        <h2>Password Reset Request</h2>
                        <p>Hello {userName},</p>
                        <p>We received a request to reset your password for your Snapdi account. Click the link below to create a new password:</p>
                        <p><a href='{resetUrl}' style='background-color: #2196F3; color: white; padding: 10px 20px; text-decoration: none; border-radius: 4px;'>Reset Password</a></p>
                        <p>Or copy and paste this link into your browser:</p>
                        <p>{resetUrl}</p>
                        <p>This reset link will expire in 1 hour.</p>
                        <p>If you didn't request a password reset, please ignore this email.</p>
                        <br>
                        <p>Best regards,<br>The Snapdi Team</p>
                    </body>
                    </html>";

                return await SendEmailAsync(toEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", toEmail);
                return false;
            }
        }

        public async Task<bool> SendWelcomeEmailAsync(string toEmail, string userName)
        {
            try
            {
                var subject = "Welcome to Snapdi!";

                var body = $@"
                    <html>
                    <body>
                        <h2>Welcome to Snapdi, {userName}!</h2>
                        <p>Your email has been successfully verified and your account is now active.</p>
                        <p>You can now:</p>
                        <ul>
                            <li>Browse and book photographers</li>
                            <li>Create your photographer profile</li>
                            <li>Connect with the photography community</li>
                        </ul>
                        <p>Thank you for joining Snapdi!</p>
                        <br>
                        <p>Best regards,<br>The Snapdi Team</p>
                    </body>
                    </html>";

                return await SendEmailAsync(toEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send welcome email to {Email}", toEmail);
                return false;
            }
        }

        public async Task<bool> SendVerificationCodeAsync(string toEmail, string userName, string verificationCode)
        {
            try
            {
                var subject = "Your Snapdi Verification Code";

                var body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <div style='text-align: center; margin-bottom: 30px;'>
                            <h1 style='color: #4CAF50; margin-bottom: 10px;'>Snapdi</h1>
                            <h2 style='color: #333; font-weight: normal;'>Email Verification</h2>
                        </div>
                        
                        <div style='background-color: #f9f9f9; padding: 20px; border-radius: 8px; margin-bottom: 20px;'>
                            <p style='color: #333; font-size: 16px; margin-bottom: 20px;'>Hello {userName},</p>
                            <p style='color: #333; font-size: 16px; margin-bottom: 20px;'>
                                Thank you for registering with Snapdi! To complete your registration, please enter the verification code below in the app:
                            </p>
                            
                            <div style='text-align: center; margin: 30px 0;'>
                                <div style='background-color: #4CAF50; color: white; font-size: 32px; font-weight: bold; padding: 20px; border-radius: 8px; letter-spacing: 8px; display: inline-block;'>
                                    {verificationCode}
                                </div>
                            </div>
                            
                            <p style='color: #666; font-size: 14px; text-align: center; margin-top: 20px;'>
                                This verification code will expire in 15 minutes.
                            </p>
                        </div>
                        
                        <div style='background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 4px; margin-bottom: 20px;'>
                            <p style='color: #856404; font-size: 14px; margin: 0;'>
                                <strong>Security Tip:</strong> Never share this code with anyone. Snapdi staff will never ask for your verification code.
                            </p>
                        </div>
                        
                        <p style='color: #666; font-size: 14px;'>
                            If you didn't create an account with Snapdi, please ignore this email.
                        </p>
                        
                        <div style='text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee;'>
                            <p style='color: #999; font-size: 12px; margin: 0;'>
                                Best regards,<br>
                                The Snapdi Team
                            </p>
                        </div>
                    </body>
                    </html>";

                return await SendEmailAsync(toEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification code email to {Email}", toEmail);
                return false;
            }
        }

        public async Task<bool> SendPasswordResetCodeAsync(string toEmail, string userName, string resetCode)
        {
            try
            {
                var subject = "Your Snapdi Password Reset Code";

                var body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <div style='text-align: center; margin-bottom: 30px;'>
                            <h1 style='color: #2196F3; margin-bottom: 10px;'>Snapdi</h1>
                            <h2 style='color: #333; font-weight: normal;'>Password Reset</h2>
                        </div>
                        
                        <div style='background-color: #f9f9f9; padding: 20px; border-radius: 8px; margin-bottom: 20px;'>
                            <p style='color: #333; font-size: 16px; margin-bottom: 20px;'>Hello {userName},</p>
                            <p style='color: #333; font-size: 16px; margin-bottom: 20px;'>
                                We received a request to reset your password for your Snapdi account. Please enter the code below to reset your password:
                            </p>
                            
                            <div style='text-align: center; margin: 30px 0;'>
                                <div style='background-color: #2196F3; color: white; font-size: 32px; font-weight: bold; padding: 20px; border-radius: 8px; letter-spacing: 8px; display: inline-block;'>
                                    {resetCode}
                                </div>
                            </div>
                            
                            <p style='color: #666; font-size: 14px; text-align: center; margin-top: 20px;'>
                                This reset code will expire in 15 minutes.
                            </p>
                        </div>
                        
                        <div style='background-color: #f8d7da; border: 1px solid #f5c6cb; padding: 15px; border-radius: 4px; margin-bottom: 20px;'>
                            <p style='color: #721c24; font-size: 14px; margin: 0;'>
                                <strong>Security Warning:</strong> If you didn't request a password reset, please ignore this email and ensure your account is secure.
                            </p>
                        </div>
                    
                        <p style='color: #666; font-size: 14px;'>
                            If you didn't request this password reset, no action is required.
                        </p>
                            
                        <div style='text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee;'>
                            <p style='color: #999; font-size: 12px; margin: 0;'>
                                Best regards,<br>
                                The Snapdi Team
                            </p>
                        </div>
                    </body>
                    </html>";

                return await SendEmailAsync(toEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset code email to {Email}", toEmail);
                return false;
            }
        }

        private async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                using var client = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort)
                {
                    Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.FromEmail ?? _emailSettings.SmtpUsername, _emailSettings.FromName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}. SMTP Settings: Host={Host}, Port={Port}, Username={Username}",
                    toEmail, _emailSettings.SmtpHost, _emailSettings.SmtpPort, _emailSettings.SmtpUsername);
                return false;
            }
        }
    }
}