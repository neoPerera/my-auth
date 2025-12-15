namespace APPLICATION.DTOs;

public class ValidateResponse
{
    public bool IsValid { get; set; }
    public string? Message { get; set; }
    public UserInfo? User { get; set; }
    public Dictionary<string, string>? Claims { get; set; }
}

