using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace ClientOpsPortal.Services.Auth.Services;

public class RsaKeyProvider
{
    private readonly ILogger<RsaKeyProvider> _logger;

    public RsaSecurityKey SecurityKey { get; }
    public string KeyId { get; }

    public RsaKeyProvider(IHostEnvironment environment, IConfiguration configuration, ILogger<RsaKeyProvider> logger)
    {
        _logger = logger;
        var configuredPath = configuration["Jwt:KeyPath"];

        var keyPath = !string.IsNullOrWhiteSpace(configuredPath)
            ? configuredPath
            : environment.IsDevelopment()
                ? Path.Combine(Path.GetTempPath(), "clientops-portal", "jwt-signing.pem")
                : "/app/keys/jwt-signing.pem";

        var keyDir = Path.GetDirectoryName(keyPath)!;
        Directory.CreateDirectory(keyDir);

        var rsa = RSA.Create(2048);
        if (File.Exists(keyPath))
        {
            rsa.ImportFromPem(File.ReadAllText(keyPath));
            _logger.LogDebug("Loaded JWT signing key from {KeyPath}", keyPath);
        }
        else
        {
            var pem = rsa.ExportRSAPrivateKeyPem();
            File.WriteAllText(keyPath, pem);
            _logger.LogInformation("Generated new JWT signing key at {KeyPath}", keyPath);
        }

        KeyId = ComputeKeyId(rsa);
        SecurityKey = new RsaSecurityKey(rsa) { KeyId = KeyId };
        _logger.LogInformation("JWT signing key initialized with key id {KeyId}", KeyId);
    }

    private static string ComputeKeyId(RSA rsa)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(rsa.ExportRSAPublicKey());
        return Convert.ToHexString(hash)[..16];
    }
}
