using System.ComponentModel.DataAnnotations;
using PGDeployer;
using WordoGuessr.API.BuildingBlocks.Configuration;

namespace WordoGuessr.WebAPI.Migrations;

internal sealed class SchemaMigratorOptions : INamedOptions
{
    public static string Name => "SchemaMigrator";

    [Required]
    public required string ConnectionString { get; set; }

    public bool EnsureDatabase { get; set; }

    public string? JournalSchema { get; set; }

    [Required]
    public required string JournalTable { get; set; }

    public required AssemblyScriptsSource[] ScriptsSources { get; set; }

    public bool StandaloneMigrationModeEnabled { get; set; }
}
