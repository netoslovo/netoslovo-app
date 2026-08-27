using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using WordoGuessr.API.BuildingBlocks.Configuration;
using WordoGuessr.API.BuildingBlocks.ModuleOrchestration;
using WordoGuessr.Email.App.Abstractions;
using WordoGuessr.Email.App.IntegrationHandlers;
using WordoGuessr.Email.Infra.Database;
using WordoGuessr.Email.Infra.EmailSending;
using WordoGuessr.Email.Infra.EmailSending.BackgroundJobs;

namespace WordoGuessr.Email.Infra;

public static class EmailInfraExtensions
{
    public static IServiceCollection AddEmailInfra(this IServiceCollection sc, string dbConnectionString)
    {
        ArgumentNullException.ThrowIfNull(dbConnectionString);

        sc.AddOptions<EmailDbContextOptions>()
            .BindNamedConfiguration()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        sc.AddOptions<EmailQueueOptions>()
            .BindNamedConfiguration()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        sc.AddOptions<EmailQueueMetricsCollectorOptions>()
            .BindNamedConfiguration()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        sc.AddOptions<EmailSenderOptions>()
            .BindNamedConfiguration()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        sc.AddOptions<SendingJobOptions>()
            .BindNamedConfiguration()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        sc.AddOptions<ArchivationJobOptions>()
            .BindNamedConfiguration()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        sc.AddOrchestratableDbContext(
            (sp, con) => ActivatorUtilities.CreateInstance<EmailDbContext>(sp, con),
            sp => ActivatorUtilities.CreateInstance<EmailDbContext>(sp, dbConnectionString));

        sc.AddScoped<IEmailQueue>(sp => sp.GetRequiredService<EmailDbContext>());
        sc.AddScoped<IEmailQueueInternal>(sp => sp.GetRequiredService<EmailDbContext>());
        sc.AddSingleton<EmailQueueMetrics>();

        sc.AddHostedService<SendingJob>();
        sc.AddHostedService<ArchivationJob>();
        sc.AddHostedService<EmailQueueMetricsCollector>();

        sc.AddSingleton<ISmtpClientFactory, SmtpClientFactory>();
        sc.AddSingleton<IEmailSender, EmailSender>();
        sc.AddSingleton<EmailMetrics>();
        sc.AddSingleton<SmtpMetrics>();

        return sc;
    }

    public static void ConfigureEmailInfra(this WolverineOptions options)
    {
        options.Discovery.IncludeAssembly(
            typeof(EnqueueEmailHandler).Assembly);
    }

    public static EFCoreTransactionConfiguration WithEmailDbContextAbstractions(this EFCoreTransactionConfiguration config)
    {
        config.WithDbContextAbstraction<IEmailQueue, EmailDbContext>();
        return config;
    }
}
