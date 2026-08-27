using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using WordoGuessr.Auth.App.Abstractions;

namespace WordoGuessr.Auth.Infra.Cryptography;

internal sealed class OtpCodeHasher : IOtpCodeHasher
{
    private readonly string _secretKey;

    public OtpCodeHasher(IOptions<OtpCodeHasherOptions> options)
    {
        _secretKey = options.Value.SecretKey;
    }

    public string Hash(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        using var hmac = new HMACSHA512(GetBytes(_secretKey));
        var hash = Convert.ToBase64String(hmac.ComputeHash(GetBytes(code)));
        return hash;
    }

    public bool Verify(string code, string hash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentNullException.ThrowIfNull(hash);

        using var hmac = new HMACSHA512(GetBytes(_secretKey));
        var computedHash = Convert.ToBase64String(hmac.ComputeHash(GetBytes(code)));
        return hash == computedHash;
    }

    private static byte[] GetBytes(string value) => Encoding.UTF8.GetBytes(value);
}
