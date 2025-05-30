namespace ServerSide.Models;

public class JWTSettings
{
    public const string SectionName = "JWTSettings";
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationInDays { get; set; }
    public string PasswordSalt { get; set; } = string.Empty;
}
