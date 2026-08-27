using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordoGuessr.Game.ReadModels.Stored;

namespace WordoGuessr.Game.Infra.EntityConfigurations;

internal sealed class ArcadeGameStatsConfiguration
    : IEntityTypeConfiguration<ArcadeGameStats>
{
    public void Configure(EntityTypeBuilder<ArcadeGameStats> builder)
    {
        builder.ToTable("arcade_game_stats");

        builder.HasKey(ags => new { ags.PlayerId, ags.Difficulty });

        builder.Property(ags => ags.PlayerId)
            .HasColumnName("player_id");

        builder.Property(ags => ags.Difficulty)
            .HasColumnName("difficulty_code");

        builder.Property(ags => ags.GuessedGames)
            .HasColumnName("guessed_games");

        builder.Property(ags => ags.TotalAttempts)
            .HasColumnName("total_attempts");

        builder.Property(ags => ags.TotalDuration)
            .HasColumnName("total_duration");

        builder.Property(ags => ags.TotalScore)
            .HasColumnName("total_score");

        builder.Property(ags => ags.AverageScore)
            .HasColumnName("average_score")
            .HasPrecision(11, 1);

        builder.Property(ags => ags.AverageDuration)
            .HasColumnName("average_duration");

        builder.Property<uint>("version")
            .IsRowVersion();
    }
}
