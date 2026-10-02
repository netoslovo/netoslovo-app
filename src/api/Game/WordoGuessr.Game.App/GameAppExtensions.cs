using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using WordoGuessr.API.BuildingBlocks.CQRS;
using WordoGuessr.Common.Domain;
using WordoGuessr.Game.App.UseCases.Admin.Daily.Schedule.AssignSourceToDay;
using WordoGuessr.Game.App.UseCases.Admin.Daily.Schedule.AutoAssignSourcesToEmptyDays;
using WordoGuessr.Game.App.UseCases.Admin.Daily.Schedule.GetDailyGamesSchedule;
using WordoGuessr.Game.App.UseCases.Admin.Daily.Schedule.GetUnassignedDaysCount;
using WordoGuessr.Game.App.UseCases.Admin.Daily.Schedule.UnassignSourceFromDay;
using WordoGuessr.Game.App.UseCases.Admin.Daily.Sources.GetApprovedUnassignedDailySourcesCount;
using WordoGuessr.Game.App.UseCases.Admin.Daily.Sources.GetDailySourceReviews;
using WordoGuessr.Game.App.UseCases.Admin.Daily.Sources.GetGameSourceForReview;
using WordoGuessr.Game.App.UseCases.Admin.Daily.Sources.GetRandomUnreviewedGameSourceId;
using WordoGuessr.Game.App.UseCases.Admin.Daily.Sources.GetSourceByWordWithReview;
using WordoGuessr.Game.App.UseCases.Admin.Daily.Sources.ReviewDailyGameSource;
using WordoGuessr.Game.App.UseCases.Difficulties;
using WordoGuessr.Game.App.UseCases.SingleGames.Arcade.Create;
using WordoGuessr.Game.App.UseCases.SingleGames.Arcade.GetById;
using WordoGuessr.Game.App.UseCases.SingleGames.Arcade.GetHistory;
using WordoGuessr.Game.App.UseCases.SingleGames.Arcade.GetLatestActive;
using WordoGuessr.Game.App.UseCases.SingleGames.Common.MakeGuess;
using WordoGuessr.Game.App.UseCases.SingleGames.Common.RevealHalfwayWord;
using WordoGuessr.Game.App.UseCases.SingleGames.Common.RevealRandomLetter;
using WordoGuessr.Game.App.UseCases.SingleGames.Common.RevealWordLength;
using WordoGuessr.Game.App.UseCases.SingleGames.Common.Surrender;
using WordoGuessr.Game.App.UseCases.SingleGames.Daily;
using WordoGuessr.Game.App.UseCases.SingleGames.Daily.GetForDay;
using WordoGuessr.Game.App.UseCases.SingleGames.Daily.GetHistory;
using WordoGuessr.Game.App.UseCases.SingleGames.Daily.GetShare;
using WordoGuessr.Game.App.UseCases.SingleGames.Daily.GetSharedGame;
using WordoGuessr.Game.App.UseCases.SingleGames.Daily.GetToday;
using WordoGuessr.Game.App.UseCases.SingleGames.Daily.Share;
using WordoGuessr.Game.App.UseCases.SingleGames.Daily.StartForDay;
using WordoGuessr.Game.App.UseCases.SingleGames.Daily.StartToday;
using WordoGuessr.Game.App.UseCases.SingleGames.Daily.Unshare;
using WordoGuessr.Game.App.UseCases.Statistics.GetArcadeGameTopPlayers;
using WordoGuessr.Game.App.UseCases.Statistics.GetDailyGameCurrentPlayerStreak;
using WordoGuessr.Game.App.UseCases.Statistics.GetDailyGameCurrentStreakTop;
using WordoGuessr.Game.App.UseCases.Statistics.GetDailyGameLongestStreakTop;
using WordoGuessr.Game.App.UseCases.Statistics.GetDailyGamePlayerStats;
using WordoGuessr.Game.App.UseCases.Statistics.GetDailyGameStats;
using WordoGuessr.Game.Dto;

namespace WordoGuessr.Game.App;

public static class GameAppExtensions
{
    public static IServiceCollection AddGameApp(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateArcadeValidator>(includeInternalTypes: true);
        services.AddSingleton<GameAppMetrics>();

        services
            .AddCommandHandler<CreateArcadeHandler, CreateArcadeCommand, Result<GameDto, CreateArcadeError>>()
            .AddCommandHandler<StartForDayDailyHandler, StartForDayDailyCommand, Result<GameDto, StartDailyError>>()
            .AddCommandHandler<StartTodayDailyHandler, StartTodayDailyCommand, Result<GameDto, StartDailyError>>()
            .AddCommandHandler<MakeGuessHandler, MakeGuessCommand, Result<GuessOutcomeDto, MakeGuessError>>()
            .AddCommandHandler<SurrenderHandler, SurrenderCommand, Result<SurrenderError>>()
            .AddCommandHandler<RevealHalfwayWordHandler, RevealHalfwayWordHintCommand, Result<GuessHintDto, RevealHalfwayWordHintError>>()
            .AddCommandHandler<RevealWordLengthHandler, RevealWordLengthCommand, Result<TextHintDto, RevealWordLengthError>>()
            .AddCommandHandler<RevealRandomLetterHandler, RevealRandomLetterCommand, Result<TextHintDto, RevealRandomLetterError>>()
            .AddCommandHandler<ReviewDailyGameSourceHandler, ReviewDailyGameSourceCommand, Result<ReviewDailyGameSourceError>>()
            .AddCommandHandler<ShareHandler, ShareCommand, Result<Guid, ShareError>>()
            .AddCommandHandler<UnshareHandler, UnshareCommand>()

            .AddCommandHandler<AutoAssignSourcesToEmptyDaysHandler, AutoAssignSourcesToEmptyDaysCommand, Result<AutoAssignSourcesToEmptyDaysError>>()
            .AddCommandHandler<UnassignSourceFromDayHandler, UnassignSourceFromDayCommand, Result<UnassignSourceFromDayError>>()
            .AddCommandHandler<AssignSourceToDayHandler, AssignSourceToDayCommand, Result<AssignSourceToDayError>>();

        services
            .AddQueryHandler<GetLatestActiveArcadeHandler, GetLatestActiveArcadeQuery, ArcadeGameInfoDto?>()
            .AddQueryHandler<GetByIdArcadeHandler, GetByIdArcadeQuery, GameDto?>()
            .AddQueryHandler<GetShareHandler, GetShareQuery, DailyGameShareDto?>()
            .AddQueryHandler<GetSharedGameHandler, GetSharedGameQuery, Result<SharedDailyGameDto, GetSharedGameError>>()
            .AddQueryHandler<GetDailyGameForDayHandler, GetDailyGameForDayQuery, Result<GameDto?, GetDailyError>>()
            .AddQueryHandler<GetTodayDailyGameHandler, GetTodayDailyGameQuery, Result<DailyGameInfoDto?, GetDailyError>>()
            .AddQueryHandler<GetDifficultiesHandler, GetDifficultiesQuery, IReadOnlyCollection<DifficultyDto>>()

            .AddQueryHandler<GetDailyGamesHistoryHandler, GetDailyGamesHistoryQuery, DailyGamesHistoryDto>()
            .AddQueryHandler<GetArcadeGamesHistoryHandler, GetArcadeGamesHistoryQuery, ArcadeGamesHistoryDto>()

            .AddQueryHandler<GetRandomUnreviewedGameSourceIdHandler, GetRandomUnreviewedGameSourceIdQuery, long?>()
            .AddQueryHandler<GetGameSourceForReviewHandler, GetGameSourceForReviewQuery, Result<UnreviewedSourceDto, GetGameSourceForReviewError>>()
            .AddQueryHandler<GetSourceByWordWithReviewHandler, GetSourceByWordWithReviewQuery, Result<GameSourceWithReviewDto, GetSourceByWordWithReviewError>>()
            .AddQueryHandler<GetDailySourceReviewsHandler, GetDailySourceReviewsQuery, Result<DailyGameSourceReviewsDto, GetDailySourceReviewsError>>()
            .AddQueryHandler<GetApprovedUnassignedDailySourcesCountHandler, GetApprovedUnassignedDailySourcesCountQuery, int>()
            .AddQueryHandler<GetDailyGamesScheduleHandler, GetDailyGamesScheduleQuery, Result<DailyGameSchedulesDto, GetDailyGamesScheduleError>>()
            .AddQueryHandler<GetUnassignedDaysCountHandler, GetUnassignedDaysCountQuery, Result<int, GetUnassignedDaysCountError>>()

            .AddQueryHandler<GetDailyGameStatsHandler, GetDailyGameStatsQuery, Result<DailyGameStatsDto, GetDailyGameStatsError>>()
            .AddQueryHandler<GetDailyGamePlayerStatsHandler, GetDailyGamePlayerStatsQuery, Result<DailyGamePlayerStatsDto, GetDailyGamePlayerStatsError>>()
            .AddQueryHandler<GetDailyGameCurrentPlayerStreakHandler, GetDailyGameCurrentPlayerStreakQuery, DailyGameStreakDto>()
            .AddQueryHandler<GetDailyGameCurrentStreakTopHandler, GetDailyGameCurrentStreakTopQuery, DailyGameStreakTopDto>()
            .AddQueryHandler<GetDailyGameLongestStreakTopHandler, GetDailyGameLongestStreakTopQuery, DailyGameStreakTopDto>()
            .AddQueryHandler<GetArcadeGameTopPlayersHandler, GetArcadeGameTopPlayersQuery, ArcadeGameTopPlayersDto>();

        return services;
    }
}
