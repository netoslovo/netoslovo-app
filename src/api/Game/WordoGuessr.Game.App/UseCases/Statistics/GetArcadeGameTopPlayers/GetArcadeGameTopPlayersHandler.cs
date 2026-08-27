using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Auth.Contract;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.App.Mapping;
using WordoGuessr.Game.Domain;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.Statistics.GetArcadeGameTopPlayers;

internal sealed class GetArcadeGameTopPlayersHandler
    : IQueryHandler<GetArcadeGameTopPlayersQuery, ArcadeGameTopPlayersDto>
{
    private readonly IGameReadModelStore _gameReadModelStore;
    private readonly IAuthModule _authModule;

    public GetArcadeGameTopPlayersHandler(IGameReadModelStore gameReadModelStore, IAuthModule authModule)
    {
        _gameReadModelStore = gameReadModelStore ?? throw new ArgumentNullException(nameof(gameReadModelStore)); ;
        _authModule = authModule ?? throw new ArgumentNullException(nameof(authModule));
    }

    public async Task<ArcadeGameTopPlayersDto> Handle(GetArcadeGameTopPlayersQuery query, CancellationToken ct)
    {
        var difficulty = Difficulty.FromCode(query.DifficultyCode);
        var arcadeGameTopPlayers = await _gameReadModelStore.GetArcadeGameTopPlayers(
            query.PlayerId,
            difficulty,
            query.TopN,
            ct);

        var playerdIds = arcadeGameTopPlayers.Top
            .Select(x => x.PlayerId)
            .Append(arcadeGameTopPlayers.PlayerTopInfo.PlayerId)
            .Distinct();
        var playerNames = await _authModule.GetUserNames(playerdIds.ToArray(), ct);

        return arcadeGameTopPlayers.MapToDto(playerNames);
    }
}
