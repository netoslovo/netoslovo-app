using System.Data.Common;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using FakeItEasy;
using WordoGuessr.Email.Infra.Database;
using WordoGuessr.Email.Infra.EmailSending;
using WordoGuessr.Testing.Common;
using WordoGuessr.Testing.Common.PostgreSql;
using Xunit;

namespace WordoGuessr.Email.Tests;

public sealed class EmailDbContextFixture : DbContextFixture<EmailDbContext>
{
    public EmailDbContextFixture(PostgreSqlAssemblyFixture postgreSqlFixture)
        : base(
            postgreSqlFixture,
            "email_schema_migrations_journal")
    {
    }

    public EmailDbContext CreateDbContext(
        EmailDbContextOptions dbContextOptions,
        EmailQueueOptions emailQueueOptions,
        TimeProvider timeProvider)
    {
        return CreateDbContext(
            NullLoggerFactory.Instance,
            (loggerFactory, connection) => CreateEmailDbContext(
                loggerFactory,
                connection,
                dbContextOptions,
                emailQueueOptions,
                timeProvider));
    }

    public EmailDbContext CreateDbContext(
        ITestOutputHelper output,
        EmailDbContextOptions dbContextOptions,
        EmailQueueOptions emailQueueOptions,
        TimeProvider timeProvider)
    {
        return CreateDbContext(
            output,
            (loggerFactory, connection) => CreateEmailDbContext(
                loggerFactory,
                connection,
                dbContextOptions,
                emailQueueOptions,
                timeProvider));
    }

    private static EmailDbContext CreateEmailDbContext(
        ILoggerFactory loggerFactory,
        DbConnection connection,
        EmailDbContextOptions dbContextOptions,
        EmailQueueOptions emailQueueOptions,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(dbContextOptions);
        ArgumentNullException.ThrowIfNull(emailQueueOptions);
        ArgumentNullException.ThrowIfNull(timeProvider);

        return new EmailDbContext(
            Options.Create(dbContextOptions),
            loggerFactory,
            connection,
            Options.Create(emailQueueOptions),
            timeProvider,
            new EmailMetrics(CreateMeterFactory()));
    }

    private static IMeterFactory CreateMeterFactory()
    {
        var meterFactory = A.Fake<IMeterFactory>();
        A.CallTo(() => meterFactory.Create(A<MeterOptions>._))
            .Returns(new Meter($"{nameof(EmailDbContextFixture)}.{Guid.NewGuid()}"));
        return meterFactory;
    }
}
