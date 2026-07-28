namespace ClientOpsPortal.Web.Features.Auth.Models
{
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
    }
}
