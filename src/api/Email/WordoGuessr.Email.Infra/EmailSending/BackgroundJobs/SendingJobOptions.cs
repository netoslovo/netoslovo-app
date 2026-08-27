using System.ComponentModel.DataAnnotations;
using WordoGuessr.API.BuildingBlocks.Configuration;

namespace WordoGuessr.Email.Infra.EmailSending.BackgroundJobs;

internal sealed class SendingJobOptions : INamedOptions
{
    public static string Name => "Email:SendingJob";

    [Range(typeof(TimeSpan), "00:00:01", "01:00:00")]
    public TimeSpan Interval { get; set; }

    [Range(1, 1000)]
    public int BatchSize { get; set; }
}
