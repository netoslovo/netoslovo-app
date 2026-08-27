using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordoGuessr.Game.ReadModels.Stored;

namespace WordoGuessr.Game.Infra.EntityConfigurations;

internal sealed class SingleGameResultConfiguration
    : IEntityTypeConfiguration<SingleGameResult>
{
    public void Configure(EntityTypeBuilder<SingleGameResult> builder)
    {
        builder.ToTable("single_game_results");

        builder.HasKey(r => r.GameId);

        builder.Property(r => r.GameId)
            .HasColumnName("game_id");

        builder.Property(r => r.GameSourceId)
            .HasColumnName("game_source_id");

        builder.Property(r => r.Mode)
            .HasColumnName("mode");

        builder.Property(r => r.DifficultyCode)
            .HasColumnName("difficulty_code");

        builder.Property(g => g.DayOfDailyGame)
            .HasColumnName("day_of_daily_game");

        builder.Property(r => r.PlayerId)
            .HasColumnName("player_id");

        builder.Property(r => r.Duration)
            .HasColumnName("duration");

        builder.Property(r => r.Score)
            .HasColumnName("score");

        builder.Property(r => r.Attempts)
            .HasColumnName("attempts");

        builder.Property(r => r.RevealHalfwayWordHintsUsed)
            .HasColumnName("reveal_halfway_word_hints_used");

        builder.Property(r => r.RevealLengthHintUsed)
            .HasColumnName("reveal_length_hint_used");

        builder.Property(r => r.RevealLetterHintsUsed)
            .HasColumnName("reveal_letter_hints_used");

        builder.Property(r => r.State)
            .HasColumnName("state");
    }
}
