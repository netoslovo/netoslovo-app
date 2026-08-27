using WordoGuessr.API.BuildingBlocks.Configuration;

namespace WordoGuessr.Auth.Infra.Identity;

internal sealed class DefaultAdminOptions : INamedOptions
{
    public static string Name => "Auth:DefaultAdmin";

    public required string[] Emails { get; set; }

}