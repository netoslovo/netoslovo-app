using System.Data.Common;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WordoGuessr.API.BuildingBlocks.DbContextCommon;
using WordoGuessr.Auth.Domain;
using WordoGuessr.Auth.Infra.Database.EntityConfigurations;
using WordoGuessr.Common.Domain.ValueObjects;

namespace WordoGuessr.Auth.Infra.Database;

public sealed class AuthDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public AuthDbContext(
        IOptions<AuthDbContextOptions> options,
        ILoggerFactory loggerFactory,
        DbConnection connection)
    : base(DbContextOptionsFactory.Create<AuthDbContext, AuthDbContextOptions>(options, loggerFactory, connection))
    {
    }

    public AuthDbContext(
        IOptions<AuthDbContextOptions> options,
        ILoggerFactory loggerFactory,
        string connectionString)
    : base(DbContextOptionsFactory.Create<AuthDbContext, AuthDbContextOptions>(options, loggerFactory, connectionString))
    {
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<EmailAddress>()
            .HaveConversion<EmailAddressValueConverter>();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("auth");

        builder.ApplyConfiguration(new ApplicationUserConfiguration());
        builder.ApplyConfiguration(new IdentityUserClaimConfiguration());
        builder.ApplyConfiguration(new IdentityUserLoginConfiguration());
        builder.ApplyConfiguration(new IdentityUserTokenConfiguration());

        builder.ApplyConfiguration(new ApplicationRoleConfiguration());
        builder.ApplyConfiguration(new IdentityRoleClaimConfiguration());
        builder.ApplyConfiguration(new IdentityUserRoleConfiguration());

        builder.ApplyConfiguration(new OtpChallengeConfiguration());
        builder.ApplyConfiguration(new UserNameFilterVersionConfiguration());
        builder.ApplyConfiguration(new UserNoticeViewConfiguration());
        builder.ApplyConfiguration(new RecurringUserNoticeViewConfiguration());
    }
}
