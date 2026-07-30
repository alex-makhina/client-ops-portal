namespace ClientOpsPortal.Services.Auth.Contracts;

public class LoginRequest
{
    public required string Login { get; set; }
    public required string Password { get; set; }
}

public class AuthResponse
{
    public required string Token { get; set; }
    public required List<string> Roles { get; set; }
    public required string UserId { get; set; }
    public required string UserName { get; set; }
}
