namespace Snapdi.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailVerificationAsync(string toEmail, string userName, string verificationToken);
        Task<bool> SendPasswordResetAsync(string toEmail, string userName, string resetToken);
        Task<bool> SendWelcomeEmailAsync(string toEmail, string userName);
    }
}