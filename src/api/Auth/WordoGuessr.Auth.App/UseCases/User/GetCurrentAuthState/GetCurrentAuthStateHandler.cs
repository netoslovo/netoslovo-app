using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.API.BuildingBlocks.Security.CurrentPlayerAccessor;
using WordoGuessr.Auth.App.Dto;
using WordoGuessr.Auth.App.Mapping;

namespace WordoGuessr.Auth.App.UseCases.User.GetCurrentAuthState;

internal sealed class GetCurrentAuthStateHandler : IQueryHandler<GetCurrentAuthStateQuery, AuthMeDto>
{
    private readonly ICurrentPlayerAccessor _currentPlayerAccessor;

    public GetCurrentAuthStateHandler(ICurrentPlayerAccessor currentPlayerAccessor)
    {
        _currentPlayerAccessor = currentPlayerAccessor ?? throw new ArgumentNullException(nameof(currentPlayerAccessor));
    }

    public async Task<AuthMeDto> Handle(GetCurrentAuthStateQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var currentUser = _currentPlayerAccessor.GetCurrentPlayer();
        var authMe = currentUser.MapToDto();
        return authMe;
    }
}
