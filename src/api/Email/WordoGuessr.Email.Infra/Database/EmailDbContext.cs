using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WordoGuessr.API.BuildingBlocks.DbContextCommon;
using WordoGuessr.Common.Domain.ValueObjects;
using WordoGuessr.Email.Infra.Database.EntityConfigurations;
using WordoGuessr.Email.Infra.EmailSending;

namespace WordoGuessr.Email.Infra.Database;

public sealed partial class EmailDbContext : DbContext
{
    private readonly EmailMetrics _emailMetrics;

    public EmailDbContext(
        IOptions<EmailDbContextOptions> options,
        ILoggerFactory loggerFactory,
        DbConnection connection,
        IOptions<EmailQueueOptions> emailQueueOptions,
        TimeProvider timeProvider,
        EmailMetrics emailMetrics)
    : base(DbContextOptionsFactory.Create<EmailDbContext, EmailDbContextOptions>(
        options, loggerFactory, connection))
    {
        _emailQueueOptions = emailQueueOptions?.Value ?? throw new ArgumentNullException(nameof(emailQueueOptions));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _emailMetrics = emailMetrics ?? throw new ArgumentNullException(nameof(emailMetrics));
    }

    public EmailDbContext(
        IOptions<EmailDbContextOptions> options,
        ILoggerFactory loggerFactory,
        string connectionString,
        IOptions<EmailQueueOptions> emailQueueOptions,
        TimeProvider timeProvider,
        EmailMetrics emailMetrics)
    : base(DbContextOptionsFactory.Create<EmailDbContext, EmailDbContextOptions>(
        options, loggerFactory, connectionString))
    {
        _emailQueueOptions = emailQueueOptions?.Value ?? throw new ArgumentNullException(nameof(emailQueueOptions));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(emailQueueOptions));
        _emailMetrics = emailMetrics ?? throw new ArgumentNullException(nameof(emailMetrics));
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<EmailAddress>()
            .HaveConversion<EmailAddressValueConverter>();

        configurationBuilder
            .Properties<EmailSubject>()
            .HaveConversion<EmailSubjectValueConverter>();

        configurationBuilder
            .Properties<EmailHtmlBody>()
            .HaveConversion<EmailHtmlBodyValueConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("email");

        modelBuilder.ApplyConfiguration(new EmailMessageConfiguration());
        modelBuilder.ApplyConfiguration(new EmailMessageHistoryConfiguration());
    }
}
