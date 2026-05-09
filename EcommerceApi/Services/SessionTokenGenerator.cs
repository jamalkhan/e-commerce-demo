using System.Security.Cryptography;

namespace EcommerceApi.Services;

public interface ISecureTokenGenerator
{
    string Generate(int byteLength = 32);
}

public class SecureTokenGenerator : ISecureTokenGenerator
{
    public string Generate(int byteLength = 32)
    {
        if (byteLength <= 0) throw new ArgumentOutOfRangeException(nameof(byteLength));
        var bytes = RandomNumberGenerator.GetBytes(byteLength);
        return Base64UrlEncode(bytes);
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
