namespace ClientOpsPortal.Services.Auth.Settings;

public sealed class AuthSettings
{
    public const string SectionName = "Auth";

    public string Issuer { get; init; } = "http://localhost:5110";
    public string AdminPortalUrl { get; init; } = "http://localhost:5022";
    public string PersonalAccountUrl { get; init; } = "http://localhost:62000";

    public IReadOnlyList<string> AllowedOrigins { get; }

    public AuthSettings()
    {
        AllowedOrigins = BuildAllowedOrigins();
    }

    public string GetPortalUrl(IEnumerable<string> roles) =>
        roles.Contains("Abonent") ? PersonalAccountUrl : AdminPortalUrl;

    public bool IsAllowedReturnUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        if (url.StartsWith('/') && !url.StartsWith("//"))
            return true;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var absolute))
            return false;

        return AllowedOrigins.Contains($"{absolute.Scheme}://{absolute.Authority}", StringComparer.OrdinalIgnoreCase);
    }

    private IReadOnlyList<string> BuildAllowedOrigins()
    {
        var origins = new List<string>();
        AddPortalOrigin(origins, AdminPortalUrl);
        AddPortalOrigin(origins, PersonalAccountUrl);
        return origins.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static void AddPortalOrigin(List<string> origins, string portalUrl)
    {
        if (!Uri.TryCreate(portalUrl, UriKind.Absolute, out var uri))
            return;

        var origin = $"{uri.Scheme}://{uri.Authority}";
        origins.Add(origin);

        var aliasHost = uri.Host.ToLowerInvariant() switch
        {
            "localhost" => "127.0.0.1",
            "127.0.0.1" => "localhost",
            _ => null
        };

        if (aliasHost is not null)
            origins.Add($"{uri.Scheme}://{aliasHost}:{uri.Port}");
    }
}
