namespace ClientOpsPortal.Services.Auth.Contracts;

public class ForgotPasswordRequest
{
    public required string LoginIdentifier { get; set; }
}

public class ForgotPasswordResponse
{
    public string TemporaryPassword { get; set; } = string.Empty;
}
