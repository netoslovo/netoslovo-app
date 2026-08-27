using PGDeployer;
using WordoGuessr.API.BuildingBlocks.Configuration;
using WordoGuessr.Auth.Infra.Database;
using WordoGuessr.Email.Infra.Database;
using WordoGuessr.Game.Infra;
using WordoGuessr.Sagas.Infra.Storage;
using WordoGuessr.WebAPI.Messaging;
using WordoGuessr.Words.Infra.Database;

namespace WordoGuessr.WebAPI.Migrations;

internal static class MigrationExtensions
{
    public static IHostApplicationBuilder AddEmbeddedSchemaMigration(
        this IHostApplicationBuilder builder,
        string connectionString,
        bool standaloneMigrationModeEnabled = false)
    {
        builder.Services.AddHostedService<SchemaMigrationHostedService>();
        builder.Services.AddOptions<SchemaMigratorOptions>()
            .BindNamedConfiguration()
            .Configure(options =>
            {
                options.ConnectionString = connectionString;
                options.ScriptsSources =
                [
                    new AssemblyScriptsSource(typeof(MessagingExtensions).Assembly),
                    new AssemblyScriptsSource(typeof(GameDbContext).Assembly),
                    new AssemblyScriptsSource(typeof(AuthDbContext).Assembly),
                    new AssemblyScriptsSource(typeof(EmailDbContext).Assembly),
                    new AssemblyScriptsSource(typeof(WordsDbContext).Assembly),
                    new AssemblyScriptsSource(typeof(SagasDbContext).Assembly)
                ];
                options.StandaloneMigrationModeEnabled = standaloneMigrationModeEnabled;
            });

        return builder;
    }
}
