namespace ClientOpsPortal.Services.Auth.Contracts;

public class ForgotPasswordRequest
{
    public required string LoginIdentifier { get; set; }
}

public class ForgotPasswordResponse
{
    public string TemporaryPassword { get; set; } = string.Empty;
}

public class SetPasswordRequest
{
    public required string LoginIdentifier { get; set; }
    public required string Token { get; set; }
    public required string NewPassword { get; set; }
}

public class ResetPasswordRequest
{
    public required string LoginIdentifier { get; set; }
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
}
