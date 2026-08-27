using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordoGuessr.Game.Domain;

namespace WordoGuessr.Game.Infra.EntityConfigurations;

public sealed class GameSourceConfiguration : IEntityTypeConfiguration<GameSource>
{
    public void Configure(EntityTypeBuilder<GameSource> builder)
    {
        builder.ToTable("game_sources");

        builder.HasKey(gs => gs.Id);

        builder.Property(gs => gs.Id)
            .HasColumnName("id");

        builder.Property(gs => gs.Word)
            .HasColumnName("word");
    }
}
