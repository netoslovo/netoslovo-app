using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using WordoGuessr.API.BuildingBlocks.Routing;
using WordoGuessr.API.BuildingBlocks.Security.Authorization;
using WordoGuessr.Game.API.Security;
using WordoGuessr.Game.API.UseCases.Admin.Daily.AssignSourceToDay;
using WordoGuessr.Game.API.UseCases.Admin.Daily.AutoAssignSourcesToEmptyDays;
using WordoGuessr.Game.API.UseCases.Admin.Daily.GetApprovedUnassignedDailySourcesCount;
using WordoGuessr.Game.API.UseCases.Admin.Daily.GetDailyGamesSchedule;
using WordoGuessr.Game.API.UseCases.Admin.Daily.GetDailySourceReviews;
using WordoGuessr.Game.API.UseCases.Admin.Daily.GetGameSourceForReview;
using WordoGuessr.Game.API.UseCases.Admin.Daily.GetRandomUnreviewedGameSourceId;
using WordoGuessr.Game.API.UseCases.Admin.Daily.GetSourceByWordWithReview;
using WordoGuessr.Game.API.UseCases.Admin.Daily.GetUnassignedDaysCount;
using WordoGuessr.Game.API.UseCases.Admin.Daily.ReviewDailyGameSource;
using WordoGuessr.Game.API.UseCases.Admin.Daily.UnassignSourceFromDay;
using WordoGuessr.Game.API.UseCases.Dictionaries.Difficulties;
using WordoGuessr.Game.API.UseCases.SingleGames.Arcade.Create;
using WordoGuessr.Game.API.UseCases.SingleGames.Arcade.GetById;
using WordoGuessr.Game.API.UseCases.SingleGames.Arcade.GetHistory;
using WordoGuessr.Game.API.UseCases.SingleGames.Arcade.GetLatestActive;
using WordoGuessr.Game.API.UseCases.SingleGames.Common.MakeGuess;
using WordoGuessr.Game.API.UseCases.SingleGames.Common.RevealHalfwayWord;
using WordoGuessr.Game.API.UseCases.SingleGames.Common.RevealRandomLetter;
using WordoGuessr.Game.API.UseCases.SingleGames.Common.RevealWordLength;
using WordoGuessr.Game.API.UseCases.SingleGames.Common.Surrender;
using WordoGuessr.Game.API.UseCases.SingleGames.Daily.GetForDay;
using WordoGuessr.Game.API.UseCases.SingleGames.Daily.GetHistory;
using WordoGuessr.Game.API.UseCases.SingleGames.Daily.GetShare;
using WordoGuessr.Game.API.UseCases.SingleGames.Daily.GetSharedGame;
using WordoGuessr.Game.API.UseCases.SingleGames.Daily.GetToday;
using WordoGuessr.Game.API.UseCases.SingleGames.Daily.Share;
using WordoGuessr.Game.API.UseCases.SingleGames.Daily.StartForDay;
using WordoGuessr.Game.API.UseCases.SingleGames.Daily.StartToday;
using WordoGuessr.Game.API.UseCases.SingleGames.Daily.Unshare;
using WordoGuessr.Game.API.UseCases.Statistics.GetArcadeGameTopPlayers;
using WordoGuessr.Game.API.UseCases.Statistics.GetDailyGameCurrentPlayerStreak;
using WordoGuessr.Game.API.UseCases.Statistics.GetDailyGameCurrentStreakTop;
using WordoGuessr.Game.API.UseCases.Statistics.GetDailyGameLongestStreakTop;
using WordoGuessr.Game.API.UseCases.Statistics.GetDailyGamePlayerStats;
using WordoGuessr.Game.API.UseCases.Statistics.GetDailyGameStats;

namespace WordoGuessr.Game.API;

public static class GameApiExtensions
{
    public static IServiceCollection AddGameApi(this IServiceCollection services)
    {
        services.AddTransient<IModuleClaimsTransformation, GamePermissionClaimsTransformation>();
        services.AddAuthorization(options => options.AddGamePolicies());

        services.Configure<RouteOptions>(options =>
        {
            options.ConstraintMap["DateOnly"] = typeof(DateOnlyRouteConstraint);
        });

        return services;
    }

    public static IEndpointRouteBuilder MapGameEndpoints(this IEndpointRouteBuilder app)
    {
        var gameGroup = app
            .MapGroup("/api/games")
            .WithTags("Game");

        var dictionariesGroup = gameGroup
            .MapGroup("/dictionaries");

        dictionariesGroup
            .MapGetDifficulties();

        var adminDailyGroup = gameGroup
            .MapGroup("/admin/daily")
            .RequireAuthorization()
            .WithMetadata(
                new ProducesResponseTypeAttribute(typeof(ProblemDetails), StatusCodes.Status401Unauthorized),
                new ProducesResponseTypeAttribute(typeof(ProblemDetails), StatusCodes.Status403Forbidden));

        adminDailyGroup
            .MapGetDailyGamesSchedule()
            .MapGetUnassignedDaysCount()
            .MapGetApprovedUnassignedDailySourcesCount()
            .MapGetDailySourceReviews()
            .MapAssignSourceToDay()
            .MapUnassignSourceFromDay()
            .MapAutoAssignSourcesToEmptyDays()
            .MapGetSourceByWordWithReview()
            .MapGetRandomUnreviewedGameSourceId()
            .MapGetGameSourceForReview()
            .MapReviewDailyGameSource();

        var singleGameGroup = gameGroup
            .MapGroup("/single")
            .WithMetadata(new ProducesResponseTypeAttribute(typeof(ProblemDetails), StatusCodes.Status401Unauthorized));

        singleGameGroup
            .MapCreateArcadeSingleGame()
            .MapGetByIdArcadeSingleGame()
            .MapGetLatestActiveArcadeSingleGame()
            .MapGetArcadeGamesHistory()

            .MapStartForDayDailySingleGame()
            .MapStartTodayDailySingleGame()
            .MapGetTodayDailyGame()
            .MapGetDailyGameForDay()
            .MapGetDailyGamesHistory()
            .MapGetDailyGameStats()
            .MapGetDailyGamePlayerStats()
            .MapGetDailyGameCurrentPlayerStreak()
            .MapGetDailyGameCurrentStreakTop()
            .MapGetDailyGameLongestStreakTop()
            .MapGetArcadeGameTopPlayers()

            .MapMakeGuessSingleGame()
            .MapSurrenderSingleGame()
            .MapRevealHalfwayWord()
            .MapRevealWordLengthSinglGame()
            .MapRevealRandomLetterSingleGame()

            .MapShareSingleGame()
            .MapGetShareSingleGame()
            .MapGetSharedGame()
            .MapUnshareSingleGame();

        return app;
    }
}
