using Microsoft.AspNetCore.Identity;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Auth.App.Dto;
using WordoGuessr.Auth.Domain;

namespace WordoGuessr.Auth.App.UseCases.User.GetProfile;

internal sealed class GetProfileHandler : IQueryHandler<GetProfileQuery, ProfileDto?>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GetProfileHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    public async Task<ProfileDto?> Handle(GetProfileQuery query, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(query.UserId.ToString());
        if (user is null)
        {
            return null;
        }

        var profileDto = new ProfileDto(
            Email: user.Email ?? string.Empty,
            UserName: user.UserName ?? string.Empty,
            UserNameChangedAt: user.UserNameChangedAt,
            CreatedAt: user.CreatedAt
        );

        return profileDto;
    }
}
