using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordoGuessr.Game.Domain;

namespace WordoGuessr.Game.Infra.EntityConfigurations;

public sealed class DailyGameShareConfiguration : IEntityTypeConfiguration<DailyGameShare>
{
    public void Configure(EntityTypeBuilder<DailyGameShare> builder)
    {
        builder.ToTable("daily_game_shares");

        builder.HasKey(ssg => ssg.Id);

        builder.Property(ssg => ssg.Id)
            .HasColumnName("game_id")
            .ValueGeneratedNever();

        builder.Property(ssg => ssg.PublicId)
            .HasColumnName("public_id");

        builder.HasIndex(ssg => ssg.PublicId)
            .IsUnique();

        builder.Property(ssg => ssg.CreatedAt)
            .HasColumnName("created_at");

        builder.HasOne(ssg => ssg.SingleGame)
            .WithOne()
            .HasForeignKey<DailyGameShare>(s => s.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
