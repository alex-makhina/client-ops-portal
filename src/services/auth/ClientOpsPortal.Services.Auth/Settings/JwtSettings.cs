namespace ClientOpsPortal.Services.Auth.Settings;
public class JwtSettings
{
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public int ExpiryMinutes { get; set; } = 60;
    public string? PrivateKeyPath { get; set; }
    public string? PublicKeyPath { get; set; }
}