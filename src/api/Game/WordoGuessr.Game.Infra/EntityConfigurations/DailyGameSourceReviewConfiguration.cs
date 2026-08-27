using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordoGuessr.Game.Domain;

namespace WordoGuessr.Game.Infra.EntityConfigurations;

internal sealed class DailyGameSourceReviewConfiguration
    : IEntityTypeConfiguration<DailyGameSourceReview>
{
    public void Configure(EntityTypeBuilder<DailyGameSourceReview> builder)
    {
        builder.ToTable("daily_game_source_reviews");

        builder.HasKey(a => a.GameSourceId);

        builder.Property(a => a.GameSourceId)
            .HasColumnName("game_source_id")
            .ValueGeneratedNever();

        builder.Property(a => a.WordsVersion)
            .HasColumnName("words_version");

        builder.HasOne(a => a.VersionedGameSource)
            .WithMany()
            .HasForeignKey(a => new { a.GameSourceId, a.WordsVersion })
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(a => a.UpdatedAt)
            .HasColumnName("updated_at");
    }
}
