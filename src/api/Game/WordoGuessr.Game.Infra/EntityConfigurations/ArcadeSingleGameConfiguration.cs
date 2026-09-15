using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordoGuessr.Game.Domain;

namespace WordoGuessr.Game.Infra.EntityConfigurations;

public sealed class ArcadeSingleGameConfiguration : IEntityTypeConfiguration<ArcadeSingleGame>
{
    public void Configure(EntityTypeBuilder<ArcadeSingleGame> builder)
    {

    }
}
