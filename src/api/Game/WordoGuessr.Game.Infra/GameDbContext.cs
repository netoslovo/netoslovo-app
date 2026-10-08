using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WordoGuessr.API.BuildingBlocks.DbContextCommon;
using WordoGuessr.Common.Domain.ValueObjects;
using WordoGuessr.Game.App.Abstractions;
using WordoGuessr.Game.Domain;
using WordoGuessr.Game.Infra.EntityConfigurations;
using WordoGuessr.Game.ReadModels.Stored;

namespace WordoGuessr.Game.Infra;

public sealed class GameDbContext : DbContext, IGameStore
{
    public GameDbContext(
        IOptions<GameDbContextOptions> options,
        ILoggerFactory loggerFactory,
        DbConnection connection)
    : base(DbContextOptionsFactory.Create<GameDbContext, GameDbContextOptions>(options, loggerFactory, connection))
    {
    }

    public GameDbContext(
        IOptions<GameDbContextOptions> options,
        ILoggerFactory loggerFactory,
        string connectionString)
    : base(DbContextOptionsFactory.Create<GameDbContext, GameDbContextOptions>(options, loggerFactory, connectionString))
    {
    }

    public DbSet<SingleGame> SingleGames { get; private set; }
    public DbSet<SingleGameDailySchedule> SingleGamesDailySchedules { get; private set; }
    public DbSet<DailyGameSourceReview> DailyGameSourceReviews { get; private set; }
    public DbSet<ApprovedDailyGameSource> ApprovedDailyGameSources { get; private set; }
    public DbSet<GameSource> GameSources { get; private set; }
    public DbSet<VersionedGameSource> VersionedGameSources { get; private set; }
    public DbSet<DailyGameShare> DailyGameShares { get; private set; }
    internal DbSet<SingleGameResult> SingleGameResults { get; set; }
    internal DbSet<DailyGameStreakInfo> DailyGameStreaksInfo { get; set; }
    internal DbSet<ArcadeGameStats> ArcadeGameStats { get; set; }

    public IQueryable<SingleGame> SingleGamesForAction() =>
        SingleGamesWithSource()
            .AsSplitQuery()
            .Include(g => g.Guesses);

    public IQueryable<SingleGame> SingleGamesForDetails() =>
        SingleGamesWithSource()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(g => g.Guesses);

    public IQueryable<SingleGame> SingleGamesForSummary() =>
        SingleGamesWithSource()
            .AsNoTracking();

    public IQueryable<VersionedGameSource> VersionedGameSourcesForGameCreation() =>
        VersionedGameSources
            .Include(vgs => vgs.GameSource);

    private IQueryable<SingleGame> SingleGamesWithSource() =>
        SingleGames
            .Include(g => g.VersionedGameSource)
                .ThenInclude(source => source.GameSource);

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<Word>()
            .HaveConversion<WordValueConverter>();

        configurationBuilder
            .Properties<Difficulty>()
            .HaveConversion<DifficultyValueConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("game");

        modelBuilder.ApplyConfiguration(new GameSourceConfiguration());
        modelBuilder.ApplyConfiguration(new VersionedGameSourceConfiguration());
        modelBuilder.ApplyConfiguration(new SingleGameConfiguration());
        modelBuilder.ApplyConfiguration(new GuessConfiguration());
        modelBuilder.ApplyConfiguration(new SingleGameDailyScheduleConfiguration());
        modelBuilder.ApplyConfiguration(new DailyGameSourceReviewConfiguration());
        modelBuilder.ApplyConfiguration(new ApprovedDailyGameSourceConfiguration());
        modelBuilder.ApplyConfiguration(new SingleGameResultConfiguration());
        modelBuilder.ApplyConfiguration(new DailyGameStreakInfoConfiguration());
        modelBuilder.ApplyConfiguration(new ArcadeGameStatsConfiguration());
        modelBuilder.ApplyConfiguration(new DailyGameShareConfiguration());
    }
}
