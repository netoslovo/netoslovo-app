using System.Security.Cryptography;

namespace WordoGuessr.Auth.App.Services;

public sealed class NumericOtpCodeGenerator : IOtpCodeGenerator
{
    public string Generate(int codeLength)
    {
        var upperBound = (int)Math.Pow(10, codeLength);
        var value = RandomNumberGenerator.GetInt32(0, upperBound);

        return value.ToString($"D{codeLength}");
    }
}
