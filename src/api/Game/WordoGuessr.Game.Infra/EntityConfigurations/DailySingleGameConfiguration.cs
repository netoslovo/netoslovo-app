using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordoGuessr.Game.Domain;

namespace WordoGuessr.Game.Infra.EntityConfigurations;

public sealed class DailySingleGameConfiguration : IEntityTypeConfiguration<DailySingleGame>
{
    public void Configure(EntityTypeBuilder<DailySingleGame> builder)
    {
        builder.Property(g => g.Day)
            .HasColumnName("day_of_daily_game");
    }
}
