using JasperFx;
using JasperFx.CodeGeneration;
using JasperFx.Core;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.ErrorHandling;
using Wolverine.Postgresql;
using WordoGuessr.Common.App.Exceptions;
using WordoGuessr.Email.Infra;
using WordoGuessr.Game.Infra;
using WordoGuessr.Sagas.Infra;
using WordoGuessr.Words.Infra;

namespace WordoGuessr.WebAPI.Messaging;

internal static class MessagingExtensions
{
    public static WebApplicationBuilder AddMessaging(
        this WebApplicationBuilder builder,
        string connectionString)
    {
        builder.Host.UseWolverine(options =>
        {
            options.PersistMessagesWithPostgresql(connectionString, Const.WolverineSchemaName);
            options.UseEntityFrameworkCoreTransactions()
                .WithGameDbContextAbstractions()
                .WithEmailDbContextAbstractions()
                .WithSagasDbContextAbstractions();

            options.Policies
                .OnException<Exception>(ex => ex is IConcurrencyException)
                .Or<SagaConcurrencyException>()
                .RetryWithCooldown(
                    100.Milliseconds(),
                    250.Milliseconds(),
                    500.Milliseconds(),
                    1.Seconds(),
                    3.Seconds())
                .WithBoundedJitter(0.25);

            options.Policies
                .OnException<Exception>(ex => ex is ITransientException)
                .RetryWithCooldown(
                    1.Seconds(),
                    5.Seconds(),
                    15.Seconds(),
                    1.Minutes(),
                    5.Minutes());

            options.ConfigureWordsInfra();
            options.ConfigureGameInfra();
            options.ConfigureEmailInfra();
            options.ConfigureSagasInfra();

            options.PublishFaultEvents();

            options.Policies.UseDurableLocalQueues();
            options.MultipleHandlerBehavior = MultipleHandlerBehavior.Separated;

            if (builder.Configuration.IsHostingFeatureEnabled(d => d.WolverineDynamicCodeGeneration))
            {
                options.CodeGeneration.TypeLoadMode = TypeLoadMode.Dynamic;
            }
            else
            {
                options.CodeGeneration.TypeLoadMode = TypeLoadMode.Static;
            }
        });

        builder.Services.CritterStackDefaults(options =>
        {
            options.Production.GeneratedCodeMode = TypeLoadMode.Static;
            options.Production.AssertAllPreGeneratedTypesExist = true;
        });

        return builder;
    }
}
