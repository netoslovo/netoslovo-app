using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordoGuessr.Game.Domain;

namespace WordoGuessr.Game.Infra.EntityConfigurations;

internal sealed class SingleGameDailyScheduleConfiguration
    : IEntityTypeConfiguration<SingleGameDailySchedule>
{
    public void Configure(EntityTypeBuilder<SingleGameDailySchedule> builder)
    {
        builder.ToTable("single_game_daily_schedule");

        builder.HasKey(s => s.Day);

        builder.Property(s => s.Day)
            .HasColumnName("day");

        builder.Property(s => s.ApprovedGameSourceId)
            .HasColumnName("game_source_id");

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne(s => s.ApprovedGameSource)
            .WithMany()
            .HasForeignKey(s => s.ApprovedGameSourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property<uint>("version")
            .IsRowVersion();
    }
}
