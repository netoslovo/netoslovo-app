using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordoGuessr.Game.Domain;

namespace WordoGuessr.Game.Infra.EntityConfigurations;

internal sealed class ApprovedDailyGameSourceConfiguration
    : IEntityTypeConfiguration<ApprovedDailyGameSource>
{
    public void Configure(EntityTypeBuilder<ApprovedDailyGameSource> builder)
    {
        builder.ToTable("approved_daily_game_sources");

        builder.HasKey(s => s.GameSourceId);

        builder.Property(s => s.GameSourceId)
            .HasColumnName("game_source_id")
            .ValueGeneratedNever();

        builder.HasOne(s => s.Review)
            .WithOne(r => r.ApprovedSource)
            .HasForeignKey<ApprovedDailyGameSource>(s => s.GameSourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Schedule)
            .WithOne(s => s.ApprovedGameSource)
            .HasForeignKey<SingleGameDailySchedule>(s => s.ApprovedGameSourceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
