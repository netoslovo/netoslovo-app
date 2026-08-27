using System.ComponentModel.DataAnnotations;
using WordoGuessr.API.BuildingBlocks.Configuration;

namespace WordoGuessr.Auth.Infra.Cryptography;

internal sealed class OtpCodeHasherOptions : INamedOptions
{
    public static string Name => "Auth:OtpCodeHasher";

    [Required]
    [MinLength(32)]
    public required string SecretKey { get; set; }
}