using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordoGuessr.Game.Domain;

namespace WordoGuessr.Game.Infra.EntityConfigurations;

internal sealed class VersionedGameSourceConfiguration
    : IEntityTypeConfiguration<VersionedGameSource>
{
    public void Configure(EntityTypeBuilder<VersionedGameSource> builder)
    {
        builder.ToTable("versioned_game_sources");

        builder.HasKey(gs => new { gs.GameSourceId, gs.WordsVersion });

        builder.Property(gs => gs.GameSourceId)
            .HasColumnName("game_source_id");

        builder.Property(gs => gs.WordsVersion)
            .HasColumnName("words_version");

        builder.Property(gs => gs.PredictedRawDifficultyValue)
            .HasColumnName("predicted_difficulty");

        builder.Property(gs => gs.Difficulty)
            .HasColumnName("difficulty_code");

        builder.HasOne(gs => gs.GameSource)
            .WithMany()
            .HasForeignKey(gs => gs.GameSourceId);
    }
}
