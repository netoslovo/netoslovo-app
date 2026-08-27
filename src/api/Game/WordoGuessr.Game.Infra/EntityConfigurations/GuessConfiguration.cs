using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordoGuessr.Game.Domain;

namespace WordoGuessr.Game.Infra.EntityConfigurations;

public sealed class GuessConfiguration : IEntityTypeConfiguration<Guess>
{
    public void Configure(EntityTypeBuilder<Guess> builder)
    {
        builder.ToTable("guesses");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Id)
            .HasColumnName("id");

        builder.Property(gs => gs.Word)
            .HasColumnName("word");

        builder.Property(g => g.Distance)
            .HasColumnName("distance");

        builder.Property(g => g.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(g => g.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(g => g.Source)
            .HasColumnName("source");
    }
}
