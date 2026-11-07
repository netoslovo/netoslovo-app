using System.Data.Common;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WordoGuessr.API.BuildingBlocks.DbContextCommon;

namespace WordoGuessr.WebAPI.DataProtection;

internal sealed class DataProtectionDbContext : DbContext, IDataProtectionKeyContext
{
    public DataProtectionDbContext(
        IOptions<DataProtectionDbContextOptions> options,
        ILoggerFactory loggerFactory,
        DbConnection connection)
    : base(DbContextOptionsFactory.Create<DataProtectionDbContext, DataProtectionDbContextOptions>(
        options,
        loggerFactory,
        connection))
    { }

    public DataProtectionDbContext(
        IOptions<DataProtectionDbContextOptions> options,
        ILoggerFactory loggerFactory,
        string connectionString)
    : base(DbContextOptionsFactory.Create<DataProtectionDbContext, DataProtectionDbContextOptions>(
        options,
        loggerFactory,
        connectionString))
    { }

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("data_protection");

        modelBuilder.ApplyConfiguration(new DataProtectionKeyConfiguration());
    }

}