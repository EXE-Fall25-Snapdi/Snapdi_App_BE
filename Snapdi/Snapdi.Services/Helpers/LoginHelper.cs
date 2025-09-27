using System.Text.RegularExpressions;

namespace Snapdi.Services.Helpers
{
    public static class LoginHelper
    {
        /// <summary>
        /// Determines if the input string is an email address
        /// </summary>
        /// <param name="input">The input string to check</param>
        /// <returns>True if the input appears to be an email address</returns>
        public static bool IsEmail(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            // Simple email validation - contains @ symbol and has domain
            return input.Contains("@") && input.Contains(".");
        }

        /// <summary>
        /// Determines if the input string is a phone number
        /// </summary>
        /// <param name="input">The input string to check</param>
        /// <returns>True if the input appears to be a phone number</returns>
        public static bool IsPhoneNumber(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            // Remove common phone number characters
            var cleanInput = Regex.Replace(input, @"[\s\-\(\)\+]", "");
            
            // Check if it's all digits and has reasonable length (7-15 digits)
            return Regex.IsMatch(cleanInput, @"^\d{7,15}$");
        }

        /// <summary>
        /// Normalizes phone number by removing formatting characters
        /// </summary>
        /// <param name="phoneNumber">Raw phone number input</param>
        /// <returns>Normalized phone number with only digits and + prefix if international</returns>
        public static string NormalizePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                return string.Empty;

            // Keep only digits and + at the beginning
            var normalized = Regex.Replace(phoneNumber, @"[^\d\+]", "");
            
            // If it starts with +, keep it, otherwise remove any +
            if (normalized.StartsWith("+"))
            {
                normalized = "+" + Regex.Replace(normalized.Substring(1), @"\+", "");
            }
            else
            {
                normalized = Regex.Replace(normalized, @"\+", "");
            }

            return normalized;
        }
    }
}