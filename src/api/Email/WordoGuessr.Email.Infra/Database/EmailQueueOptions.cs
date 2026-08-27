using System.ComponentModel.DataAnnotations;
using WordoGuessr.API.BuildingBlocks.Configuration;

namespace WordoGuessr.Email.Infra.Database;

public sealed class EmailQueueOptions : INamedOptions
{
    public static string Name => "Email:Queue";

    [Range(1, 20)]
    public int MaxSendingAttempts { get; set; }

    [Range(typeof(TimeSpan), "00:00:01", "01:00:00")]
    public TimeSpan SendingHold { get; set; }

    [Range(typeof(TimeSpan), "00:00:01", "01:00:00")]
    public TimeSpan RetryCooldown { get; set; }

    [Range(typeof(TimeSpan), "00:01:00", "365.00:00:00")]
    public TimeSpan DeduplicationPeriod { get; set; }
}
