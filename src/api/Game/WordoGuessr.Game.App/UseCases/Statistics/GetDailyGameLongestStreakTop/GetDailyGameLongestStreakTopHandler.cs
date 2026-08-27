using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Auth.Contract;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.App.Mapping;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.Statistics.GetDailyGameLongestStreakTop;

internal sealed class GetDailyGameLongestStreakTopHandler
    : IQueryHandler<GetDailyGameLongestStreakTopQuery, DailyGameStreakTopDto>
{
    private readonly IGameReadModelStore _gameReadModelStore;
    private readonly IAuthModule _authModule;

    public GetDailyGameLongestStreakTopHandler(
        IGameReadModelStore gameReadModelStore,
        IAuthModule authModule)
    {
        _gameReadModelStore = gameReadModelStore ?? throw new ArgumentNullException(nameof(gameReadModelStore));
        _authModule = authModule ?? throw new ArgumentNullException(nameof(authModule));
    }

    public async Task<DailyGameStreakTopDto> Handle(GetDailyGameLongestStreakTopQuery query, CancellationToken ct)
    {
        var longestStreakTop = await _gameReadModelStore
            .GetDailyGameLongestStreakTop(query.PlayerId, query.TopN, ct);

        var playerdIds = longestStreakTop.Top
            .Select(x => x.PlayerId)
            .Append(longestStreakTop.PlayerStreakTopInfo.PlayerId)
            .Distinct();

        var playersNames = await _authModule.GetUserNames(playerdIds.ToArray(), ct);

        return longestStreakTop.MapToDto(playersNames);
    }
}
