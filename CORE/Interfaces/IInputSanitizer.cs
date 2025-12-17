namespace CORE.Interfaces;

public interface IInputSanitizer
{
    string SanitizeUsername(string username);
    string SanitizePassword(string password);
    string SanitizeToken(string token);
    string SanitizeString(string input, bool allowSpecialChars = false);
    bool ContainsMaliciousContent(string input);
}

