using Microsoft.EntityFrameworkCore;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Auth.Contract;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.App.Mapping;
using WordoGuessr.Game.App.Services;
using WordoGuessr.Game.Domain;
using WordoGuessr.Game.Dto;
using WordoGuessr.Words.Contract;
using StatisticsConst = WordoGuessr.Game.App.UseCases.Statistics.Const;

namespace WordoGuessr.Game.App.UseCases.SingleGames.Daily.GetSharedGame;

internal sealed class GetSharedGameHandler
    : IQueryHandler<GetSharedGameQuery, Result<SharedDailyGameDto, GetSharedGameError>>
{
    private readonly IGameStore _dbContext;
    private readonly IAuthModule _authModule;
    private readonly IGameReadModelStore _gameReadStore;
    private readonly IWordsModule _wordsModule;
    private readonly TimeProvider _timeProvider;

    public GetSharedGameHandler(
        IGameStore dbContext,
        IAuthModule authModule,
        IGameReadModelStore gameReadStore,
        IWordsModule wordsModule,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _authModule = authModule ?? throw new ArgumentNullException(nameof(authModule));
        _gameReadStore = gameReadStore ?? throw new ArgumentNullException(nameof(gameReadStore));
        _wordsModule = wordsModule ?? throw new ArgumentNullException(nameof(wordsModule));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<Result<SharedDailyGameDto, GetSharedGameError>> Handle(
        GetSharedGameQuery query,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);

        var now = _timeProvider.GetUtcNow();
        var today = DailyGameClock.GetDateOnly(now);

        var share = await _dbContext.DailyGameShares
            .AsNoTracking()
            .AsSplitQuery()
            .Include(sgs => sgs.SingleGame)
                .ThenInclude(sg => sg.VersionedGameSource)
                    .ThenInclude(vgs => vgs.GameSource)
            .Include(sgs => sgs.SingleGame)
                .ThenInclude(sg => sg.Guesses)
            .SingleOrDefaultAsync(
                sgs =>
                    sgs.PublicId == query.PublicId &&
                    sgs.SingleGame.Mode == SingleGameMode.Daily,
                ct);

        if (share is null)
        {
            return Result<SharedDailyGameDto, GetSharedGameError>.Failure(GetSharedGameError.GameNotFound);
        }

        var game = share.SingleGame;
        var playerName = await _authModule.GetUserName(game.PlayerId, ct)
            ?? StatisticsConst.UnknownPlayerNamePlaceholder;

        var wordsData = await _wordsModule.LoadData(
            new WordsDataRequest(TotalWords: new RequestOperation.TotalWords()),
            ct);

        return await BuildDailyDto(
            share,
            playerName,
            query.ViewerPlayerId,
            wordsData.TotalWords,
            today,
            ct);
    }

    private async Task<Result<SharedDailyGameDto, GetSharedGameError>> BuildDailyDto(
        DailyGameShare share,
        string playerName,
        Guid viewerPlayerId,
        ushort totalWords,
        DateOnly today,
        CancellationToken ct)
    {
        var game = share.SingleGame;
        if (game.DayOfDailyGame is null)
        {
            throw new InvalidOperationException("Daily single game day is not filled");
        }

        var viewerGame = await GetViewerDailyGame(viewerPlayerId, game, ct);
        var canViewSpoilers = GameSpoilersPolicy.CanViewDailySpoilers(
            game,
            viewerGame,
            today,
            out var spoilersHideReason);

        var guesses = game.Guesses.MapToSharedListDto(
            totalWords,
            showWords: canViewSpoilers);

        var playerStats = await _gameReadStore.GetDailyGamePlayerStats(
            game.PlayerId,
            game.DayOfDailyGame.Value,
            ct);

        var gameStats = await _gameReadStore.GetDailyGameStats(game.DayOfDailyGame.Value, ct);

        var gameStatsDto = gameStats is not null &&
            gameStats.TotalPlays >= StatisticsConst.MinValuableStatsCount
                ? new DailyGameStatsDto(
                    gameStats.MedianScore!.Value,
                    gameStats.MedianAttempts!.Value,
                    gameStats.MedianDuration!.Value)
                : null;

        var score = game.GetScore();

        SharedDailyGameSpoilersDto spoilers = canViewSpoilers
            ? new VisibleSharedDailyGameSpoilersDto(
                game.BuildSharedGameWordDto(),
                game.Guesses.MapToVisibleSharedListDto(totalWords))
            : new HiddenSharedDailyGameSpoilersDto(
                spoilersHideReason!.Value.MapToDto(),
                game.Guesses.MapToHiddenSharedListDto(totalWords)
            );

        var result = new SharedDailyGameDto(
            game.StateCode.MapToDto(),
            guesses,
            spoilers,
            score.Value,
            score.MapToDto(),
            playerName,
            game.DayOfDailyGame.Value,
            playerStats?.MapToDto(),
            gameStatsDto);

        return Result<SharedDailyGameDto, GetSharedGameError>.Success(result);
    }

    private async Task<SingleGame?> GetViewerDailyGame(
        Guid viewerPlayerId,
        SingleGame sharedGame,
        CancellationToken ct)
    {
        if (sharedGame.PlayerId == viewerPlayerId)
        {
            return sharedGame;
        }

        return await _dbContext.SingleGamesForSummary()
            .SingleOrDefaultAsync(
                game =>
                    game.PlayerId == viewerPlayerId &&
                    game.Mode == SingleGameMode.Daily &&
                    game.DayOfDailyGame == sharedGame.DayOfDailyGame,
                ct);
    }

}
