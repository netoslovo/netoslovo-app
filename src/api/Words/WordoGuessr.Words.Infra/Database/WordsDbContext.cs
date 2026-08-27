using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WordoGuessr.API.BuildingBlocks.DbContextCommon;
using WordoGuessr.Words.App.Abstractions;
using WordoGuessr.Words.Infra.Database.EntityConfigurations;

namespace WordoGuessr.Words.Infra.Database;

public sealed class WordsDbContext : DbContext
{
    public WordsDbContext(
        IOptions<WordsDbContextOptions> options,
        ILoggerFactory loggerFactory,
        string connectionString)
        : base(DbContextOptionsFactory.Create<WordsDbContext, WordsDbContextOptions>(
            options,
            loggerFactory,
            connectionString))
    { }

    public WordsDbContext(
        IOptions<WordsDbContextOptions> options,
        ILoggerFactory loggerFactory,
        DbConnection connection)
        : base(DbContextOptionsFactory.Create<WordsDbContext, WordsDbContextOptions>(
            options,
            loggerFactory,
            connection))
    { }

    public DbSet<WordsVersion> WordsVersions { get; private set; }
    public DbSet<WordIndex> WordsIndexes { get; private set; }
    public DbSet<WordDistanceMap> WordsSnapshots { get; private set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("words");

        modelBuilder.ApplyConfiguration(new WordsVersionConfiguration());
        modelBuilder.ApplyConfiguration(new WordIndexConfiguration());
        modelBuilder.ApplyConfiguration(new WordDistanceMapConfiguration());
    }
}
