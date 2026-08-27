using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordoGuessr.Game.ReadModels.Stored;

namespace WordoGuessr.Game.Infra.EntityConfigurations;

internal sealed class DailyGameStreakInfoConfiguration
    : IEntityTypeConfiguration<DailyGameStreakInfo>
{
    public void Configure(EntityTypeBuilder<DailyGameStreakInfo> builder)
    {
        builder.ToTable("daily_game_streaks_info");

        builder.HasKey(si => si.PlayerId);

        builder.Property(si => si.PlayerId)
            .HasColumnName("player_id");

        builder.Property(si => si.CurrentStreak)
            .HasColumnName("current_streak");

        builder.Property(si => si.LongestStreak)
            .HasColumnName("longest_streak");

        builder.Property(r => r.LastSuccessDay)
            .HasColumnName("last_success_day");

        builder.Property<uint>("version")
            .IsRowVersion();
    }
}
