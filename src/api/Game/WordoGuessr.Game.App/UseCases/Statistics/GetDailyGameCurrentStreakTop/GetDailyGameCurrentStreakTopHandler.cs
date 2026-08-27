using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Auth.Contract;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.App.Mapping;
using WordoGuessr.Game.App.Services;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App.UseCases.Statistics.GetDailyGameCurrentStreakTop;

internal sealed class GetDailyGameCurrentStreakTopHandler
    : IQueryHandler<GetDailyGameCurrentStreakTopQuery, DailyGameStreakTopDto>
{
    private readonly IGameReadModelStore _gameReadModelStore;
    private readonly TimeProvider _timeProvider;
    private readonly IAuthModule _authModule;

    public GetDailyGameCurrentStreakTopHandler(
        IGameReadModelStore gameReadModelStore,
        TimeProvider timeProvider,
        IAuthModule authModule)
    {
        _gameReadModelStore = gameReadModelStore ?? throw new ArgumentNullException(nameof(gameReadModelStore));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _authModule = authModule ?? throw new ArgumentNullException(nameof(authModule));
    }

    public async Task<DailyGameStreakTopDto> Handle(GetDailyGameCurrentStreakTopQuery query, CancellationToken ct)
    {
        var now = _timeProvider.GetUtcNow();
        var today = DailyGameClock.GetDateOnly(now);

        var currentStreakTop = await _gameReadModelStore.GetDailyGameCurrentStreakTop(
            query.PlayerId, today, query.TopN, ct);

        var playerdIds = currentStreakTop.Top
            .Select(x => x.PlayerId)
            .Append(currentStreakTop.PlayerStreakTopInfo.PlayerId)
            .Distinct();
        var playerNames = await _authModule.GetUserNames(playerdIds.ToArray(), ct);

        return currentStreakTop.MapToDto(playerNames);
    }
}
