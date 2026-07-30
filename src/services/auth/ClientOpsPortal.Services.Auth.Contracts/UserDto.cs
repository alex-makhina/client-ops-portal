namespace ClientOpsPortal.Services.Auth.Contracts;

public class CreateUserRequest
{
    public required string UserName { get; set; }
    public required string Password { get; set; }
    public required string Email { get; set; }
    public required List<string> Roles { get; set; }
}

public class UserResponse
{
    public required string Id { get; set; }
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public bool IsBlocked { get; set; }
    public required List<string> Roles { get; set; }
}

public class SetUserRoleRequest
{
    public required string UserId { get; set; }
    public required string Role { get; set; }
}

public class ChangePasswordRequest
{
    public required string UserId { get; set; }
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
}
