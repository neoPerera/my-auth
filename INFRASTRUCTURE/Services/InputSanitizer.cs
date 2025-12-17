using System.Text.RegularExpressions;
using CORE.Interfaces;

namespace INFRASTRUCTURE.Services;

public class InputSanitizer : IInputSanitizer
{
    // SQL injection patterns
    private static readonly Regex SqlInjectionPattern = new Regex(
        @"(\b(SELECT|INSERT|UPDATE|DELETE|DROP|CREATE|ALTER|EXEC|EXECUTE|UNION|SCRIPT)\b|('|(\\')|(;)|(\\)|(\|)|(\*)|(%)|(\[)|(\]))|(\bOR\b.*=.*)|(\bAND\b.*=.*))",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // XSS patterns
    private static readonly Regex XssPattern = new Regex(
        @"(<script|</script>|javascript:|onerror=|onload=|onclick=|onmouseover=|onfocus=|onblur=|<iframe|</iframe>|eval\(|expression\(|vbscript:)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Dangerous characters for usernames
    private static readonly Regex DangerousUsernameChars = new Regex(
        @"[<>""'%;()&+]",
        RegexOptions.Compiled);

    public string SanitizeUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return string.Empty;

        // Trim whitespace
        var sanitized = username.Trim();

        // Remove dangerous characters
        sanitized = DangerousUsernameChars.Replace(sanitized, string.Empty);

        // Remove SQL injection patterns
        sanitized = SqlInjectionPattern.Replace(sanitized, string.Empty);

        // Remove XSS patterns
        sanitized = XssPattern.Replace(sanitized, string.Empty);

        // Remove multiple spaces and replace with single space
        sanitized = Regex.Replace(sanitized, @"\s+", " ");

        // Remove leading/trailing spaces again after replacements
        sanitized = sanitized.Trim();

        return sanitized;
    }

    public string SanitizePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return string.Empty;

        // For passwords, we only trim whitespace and check for malicious content
        // We don't remove characters as that could break legitimate passwords
        var sanitized = password.Trim();

        // Check for malicious content but don't modify the password
        if (ContainsMaliciousContent(sanitized))
        {
            // Return empty string if malicious content detected
            // The validation will catch this
            return string.Empty;
        }

        return sanitized;
    }

    public string SanitizeToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return string.Empty;

        // Trim whitespace
        var sanitized = token.Trim();

        // Remove any whitespace characters
        sanitized = Regex.Replace(sanitized, @"\s+", string.Empty);

        // Remove dangerous characters that shouldn't be in a JWT token
        sanitized = Regex.Replace(sanitized, @"[<>""'%;()&+]", string.Empty);

        return sanitized;
    }

    public string SanitizeString(string input, bool allowSpecialChars = false)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var sanitized = input.Trim();

        if (!allowSpecialChars)
        {
            // Remove dangerous characters
            sanitized = DangerousUsernameChars.Replace(sanitized, string.Empty);
        }

        // Remove SQL injection patterns
        sanitized = SqlInjectionPattern.Replace(sanitized, string.Empty);

        // Remove XSS patterns
        sanitized = XssPattern.Replace(sanitized, string.Empty);

        // Remove multiple spaces
        sanitized = Regex.Replace(sanitized, @"\s+", " ");

        return sanitized.Trim();
    }

    public bool ContainsMaliciousContent(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        // Check for SQL injection patterns
        if (SqlInjectionPattern.IsMatch(input))
            return true;

        // Check for XSS patterns
        if (XssPattern.IsMatch(input))
            return true;

        return false;
    }
}

