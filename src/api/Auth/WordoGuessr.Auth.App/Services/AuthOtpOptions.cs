using System.ComponentModel.DataAnnotations;
using WordoGuessr.API.BuildingBlocks.Configuration;

namespace WordoGuessr.Auth.App.Services;

public sealed class AuthOtpOptions : INamedOptions
{
    public static string Name => "Auth:Otp";

    [Required]
    public required string Subject { get; set; }

    [Range(4, 10)]
    public int CodeLength { get; set; } = 6;

    [Range(typeof(TimeSpan), "00:00:30", "1.00:00:00")]
    public TimeSpan TimeToLive { get; set; } = TimeSpan.FromMinutes(10);

    [Range(1, 20)]
    public int MaxAttempts { get; set; } = 5;
}
