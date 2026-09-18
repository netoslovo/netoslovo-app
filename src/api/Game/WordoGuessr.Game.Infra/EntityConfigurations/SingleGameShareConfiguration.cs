using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordoGuessr.Game.Domain;

namespace WordoGuessr.Game.Infra.EntityConfigurations;

public sealed class SingleGameShareConfiguration : IEntityTypeConfiguration<SingleGameShare>
{
    public void Configure(EntityTypeBuilder<SingleGameShare> builder)
    {
        builder.ToTable("single_game_shares");

        builder.HasKey(ssg => ssg.Id);

        builder.Property(ssg => ssg.Id)
            .HasColumnName("game_id")
            .ValueGeneratedNever();

        builder.Property(ssg => ssg.PublicId)
            .HasColumnName("public_id");

        builder.Property(ssg => ssg.ShowGuessWords)
            .HasColumnName("show_guess_words");

        builder.Property(ssg => ssg.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(ssg => ssg.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasOne(ssg => ssg.SingleGame)
            .WithOne()
            .HasForeignKey<SingleGameShare>(s => s.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property<uint>("version")
            .IsRowVersion();
    }
}
