using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WordoGuessr.API.BuildingBlocks.DbContextCommon;
using WordoGuessr.Sagas.App.Abstractions;
using WordoGuessr.Sagas.Domain.UploadWordsVersion;
using WordoGuessr.Sagas.Infra.Storage.EntityConfigurations;

namespace WordoGuessr.Sagas.Infra.Storage;

public sealed class SagasDbContext : DbContext, ISagasStore
{
    public SagasDbContext(
        IOptions<SagasDbContextOptions> options,
        ILoggerFactory loggerFactory,
        DbConnection connection)
    : base(DbContextOptionsFactory.Create<SagasDbContext, SagasDbContextOptions>(options, loggerFactory, connection))
    {
    }

    public SagasDbContext(
        IOptions<SagasDbContextOptions> options,
        ILoggerFactory loggerFactory,
        string connectionString)
    : base(DbContextOptionsFactory.Create<SagasDbContext, SagasDbContextOptions>(options, loggerFactory, connectionString))
    {
    }

    public DbSet<UploadWordsVersionSaga> UploadWordsVersionSagas { get; private set; }
    public DbSet<UploadWordsVersionSagaHistory> UploadWordsVersionSagasHistory { get; private set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("sagas");

        modelBuilder.ApplyConfiguration(new UploadWordsVersionSagaConfiguration());
        modelBuilder.ApplyConfiguration(new UploadWordsVersionSagaHistoryConfiguration());
    }
}
