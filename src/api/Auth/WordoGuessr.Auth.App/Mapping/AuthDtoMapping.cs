using WordoGuessr.API.BuildingBlocks.Security.CurrentPlayerAccessor;
using WordoGuessr.Auth.App.Dto;

namespace WordoGuessr.Auth.App.Mapping;

public static class AuthDtoMapping
{
    public static AuthMeDto MapToDto(this CurrentPlayer currentPlayer)
    {
        return currentPlayer.IsAuthenticated
            ? AuthMeDto.AuthenticatedUser(
                authenticatedUserId: currentPlayer.AuthenticatedUserId.Value,
                userName: currentPlayer.UserName,
                email: currentPlayer.Email,
                roles: currentPlayer.Roles,
                permissions: currentPlayer.Permissions)
            : AuthMeDto.Guest(currentPlayer.GuestId!.Value);
    }
}