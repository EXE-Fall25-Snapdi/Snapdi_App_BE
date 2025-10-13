using System.Collections.Concurrent;

namespace Snapdi.Services.Services
{
    /// <summary>
    /// Service for managing verification codes in memory
    /// </summary>
    public interface IVerificationCodeService
    {
        /// <summary>
        /// Generate and store a 6-digit verification code for an email
        /// </summary>
        /// <param name="email">User's email address</param>
        /// <returns>Generated 6-digit code</returns>
        string GenerateCode(string email);

        /// <summary>
        /// Verify if the provided code matches the stored code for the email
        /// </summary>
        /// <param name="email">User's email address</param>
        /// <param name="code">Code to verify</param>
        /// <returns>True if code is valid and not expired</returns>
        bool VerifyCode(string email, string code);

        /// <summary>
        /// Remove verification code for an email (after successful verification)
        /// </summary>
        /// <param name="email">User's email address</param>
        void RemoveCode(string email);

        /// <summary>
        /// Check if user can request a new code (rate limiting)
        /// </summary>
        /// <param name="email">User's email address</param>
        /// <returns>True if user can request a new code</returns>
        bool CanRequestNewCode(string email);
    }

    public class VerificationCodeService : IVerificationCodeService
    {
        private readonly ConcurrentDictionary<string, VerificationCodeData> _codes = new();
        private readonly TimeSpan _codeExpiry = TimeSpan.FromMinutes(15); // 15 minutes expiry
        private readonly TimeSpan _requestCooldown = TimeSpan.FromMinutes(1); // 1 minute between requests

        public string GenerateCode(string email)
        {
            var code = Random.Shared.Next(100000, 999999).ToString(); // 6-digit code
            var expiry = DateTime.UtcNow.Add(_codeExpiry);
            var lastRequest = DateTime.UtcNow;

            _codes.AddOrUpdate(email.ToLower(), 
                new VerificationCodeData(code, expiry, lastRequest),
                (key, oldValue) => new VerificationCodeData(code, expiry, lastRequest));

            return code;
        }

        public bool VerifyCode(string email, string code)
        {
            if (!_codes.TryGetValue(email.ToLower(), out var storedData))
                return false;

            if (DateTime.UtcNow > storedData.ExpiryTime)
            {
                _codes.TryRemove(email.ToLower(), out _);
                return false;
            }

            return storedData.Code == code;
        }

        public void RemoveCode(string email)
        {
            _codes.TryRemove(email.ToLower(), out _);
        }

        public bool CanRequestNewCode(string email)
        {
            if (!_codes.TryGetValue(email.ToLower(), out var storedData))
                return true;

            return DateTime.UtcNow > storedData.LastRequestTime.Add(_requestCooldown);
        }

        /// <summary>
        /// Clean up expired codes (can be called periodically)
        /// </summary>
        public void CleanupExpiredCodes()
        {
            var now = DateTime.UtcNow;
            var expiredKeys = _codes
                .Where(kvp => now > kvp.Value.ExpiryTime)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _codes.TryRemove(key, out _);
            }
        }

        private record VerificationCodeData(string Code, DateTime ExpiryTime, DateTime LastRequestTime);
    }
}