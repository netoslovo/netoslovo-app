using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WordoGuessr.API.BuildingBlocks.Configuration;
using WordoGuessr.API.BuildingBlocks.ModuleOrchestration;
using WordoGuessr.API.BuildingBlocks.Security.CurrentPlayerAccessor;
using WordoGuessr.Auth.App.Abstractions;
using WordoGuessr.Auth.Domain;
using WordoGuessr.Auth.Infra.Cryptography;
using WordoGuessr.Auth.Infra.CurrentPlayerAccessor;
using WordoGuessr.Auth.Infra.Database;
using WordoGuessr.Auth.Infra.Identity;
using WordoGuessr.Auth.Infra.Identity.UserNameFiltration;
using WordoGuessr.Auth.Infra.OtpRateLimit;

namespace WordoGuessr.Auth.Infra;

public static class AuthInfraExtensions
{
    public static IServiceCollection AddAuthInfra(this IServiceCollection services, string dbConnectionString)
    {
        ArgumentNullException.ThrowIfNull(dbConnectionString);

        services.AddHttpContextAccessor();

        services.AddOptions<AuthDbContextOptions>()
            .BindNamedConfiguration()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<DefaultAdminOptions>()
            .BindNamedConfiguration()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<OtpCodeHasherOptions>()
            .BindNamedConfiguration()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<OtpRateLimitOptions>()
            .BindNamedConfiguration()
            .ValidateDataAnnotations()
            .Validate(
                options => options.Cooldown <= options.Window,
                $"{OtpRateLimitOptions.Name}:Cooldown must not exceed Window.")
            .ValidateOnStart();

        services.AddOptions<UserNameFilterUpdateWatcherOptions>()
            .BindNamedConfiguration()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<UsersMetricsCollectorOptions>()
            .BindNamedConfiguration()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOrchestratableDbContext(
            (sp, con) => ActivatorUtilities.CreateInstance<AuthDbContext>(sp, con),
            sp => ActivatorUtilities.CreateInstance<AuthDbContext>(sp, dbConnectionString));

        services.AddScoped<IAuthUnitOfWork, EfAuthUnitOfWork>();
        services.AddScoped<IOtpChallengeRepository, EfOtpChallengeRepository>();
        services.AddScoped<IUserService, IdentityUserService>();
        services.AddScoped<ISessionManager, IdentitySessionManager>();
        services.AddScoped<IGuestSessionManager, GuestSessionManager>();
        services.AddScoped<ICurrentPlayerAccessor, HttpContextCurrentPlayerAccessor>();
        services.AddScoped<IOtpCodeHasher, OtpCodeHasher>();
        services.AddScoped<IOtpRateLimiter, OtpRateLimiter>();

        services.AddSingleton<UserNameFilter>();
        services.AddSingleton<IUserNameFilter>(sp => sp.GetRequiredService<UserNameFilter>());
        services.AddSingleton<IUserNameFilterLifecycle>(sp => sp.GetRequiredService<UserNameFilter>());
        services.AddScoped<IUserNameFilterVersionStore, EfUserNameFilterVersionStore>();
        services.AddScoped<IUserNameBlockList, S3BlockList>();
        services.AddScoped<IUserNameReplacementList, S3ReplacementList>();
        services.AddSingleton<UserNameFilterMetrics>();

        services.AddScoped<IUserNoticeViewStore, EfUserNoticeViewStore>();

        services.AddSingleton<AuthInfraMetrics>();

        services.AddHostedService<UserNameFilterInitService>();
        services.AddHostedService<UserNameFilterUpdateWatcher>();
        services.AddHostedService<DefaultAdminSeedService>();
        services.AddHostedService<UsersMetricsCollector>();

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddRoles<ApplicationRole>()
            .AddSignInManager()
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddUserValidator<UserValidator>()
            .AddDefaultTokenProviders();

        services.RemoveAll<IUserValidator<ApplicationUser>>();
        services.AddScoped<IUserValidator<ApplicationUser>, UserValidator>();

        services.AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddIdentityCookies();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = ".WordoGuessr.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.SlidingExpiration = true;
        });

        return services;
    }
}
