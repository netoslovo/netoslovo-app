using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WordoGuessr.API.BuildingBlocks.DbContextCommon;

public static class DbContextOptionsFactory
{
    public static DbContextOptions<TContext> Create<TContext, TOptions>(
        IOptions<TOptions> options,
        ILoggerFactory loggerFactory,
        DbConnection connection,
        Action<DbContextOptionsBuilder<TContext>>? configureAdditionalOptions = null)
    where TContext : DbContext
    where TOptions : class, IDbContextOptions
    {
        var contextOptions = options?.Value
            ?? throw new ArgumentNullException(nameof(options));

        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(connection);

        var optionsBuilder = new DbContextOptionsBuilder<TContext>();

        optionsBuilder
            .UseNpgsql(connection)
            .UseLoggerFactory(loggerFactory)
            .AddInterceptors(new PersistenceExceptionMappingInterceptor());

        if (contextOptions.EnableDebugLogging)
        {
            optionsBuilder
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging();
        }

        configureAdditionalOptions?.Invoke(optionsBuilder);

        return optionsBuilder.Options;
    }

    public static DbContextOptions<TContext> Create<TContext, TOptions>(
        IOptions<TOptions> options,
        ILoggerFactory loggerFactory,
        string connectionString,
        Action<DbContextOptionsBuilder<TContext>>? configureAdditionalOptions = null)
    where TContext : DbContext
    where TOptions : class, IDbContextOptions
    {
        var contextOptions = options?.Value
            ?? throw new ArgumentNullException(nameof(options));

        ArgumentNullException.ThrowIfNull(loggerFactory);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException(
                "Connection string cannot be empty.",
                nameof(connectionString));
        }

        var optionsBuilder = new DbContextOptionsBuilder<TContext>();

        optionsBuilder
            .UseNpgsql(connectionString)
            .UseLoggerFactory(loggerFactory)
            .AddInterceptors(new PersistenceExceptionMappingInterceptor());

        if (contextOptions.EnableDebugLogging)
        {
            optionsBuilder
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging();
        }

        configureAdditionalOptions?.Invoke(optionsBuilder);

        return optionsBuilder.Options;
    }
}