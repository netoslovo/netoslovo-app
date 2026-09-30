using Microsoft.EntityFrameworkCore;
using WordoGuessr.Game.Domain;

namespace WordoGuessr.Game.App.Abstractions;

// TODO: Aggregate Repos + Separate queries
// TODO: wrap exceptions in Common.App exceptions on reads 
public interface IGameStore
{
    DbSet<SingleGame> SingleGames { get; }
    DbSet<SingleGameDailySchedule> SingleGamesDailySchedules { get; }
    DbSet<DailyGameSourceReview> DailyGameSourceReviews { get; }
    DbSet<ApprovedDailyGameSource> ApprovedDailyGameSources { get; }
    DbSet<GameSource> GameSources { get; }
    DbSet<VersionedGameSource> VersionedGameSources { get; }
    DbSet<DailyGameShare> DailyGameShares { get; }

    IQueryable<SingleGame> SingleGamesForAction();
    IQueryable<SingleGame> SingleGamesForDetails();
    IQueryable<SingleGame> SingleGamesForSummary();
    IQueryable<VersionedGameSource> VersionedGameSourcesForGameCreation();


}
