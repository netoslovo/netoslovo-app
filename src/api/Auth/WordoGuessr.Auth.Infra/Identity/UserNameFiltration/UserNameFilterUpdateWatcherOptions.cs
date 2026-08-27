using System.ComponentModel.DataAnnotations;
using WordoGuessr.API.BuildingBlocks.Configuration;

namespace WordoGuessr.Auth.Infra.Identity.UserNameFiltration;

internal sealed class UserNameFilterUpdateWatcherOptions : INamedOptions
{
    public static string Name => "Auth:UserNameFilterUpdateWatcher";

    [Range(typeof(TimeSpan), "00:00:01", "01:00:00")]
    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(5);
}
