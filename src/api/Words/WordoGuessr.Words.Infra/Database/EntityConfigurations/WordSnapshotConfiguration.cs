using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordoGuessr.Words.App.Abstractions;

namespace WordoGuessr.Words.Infra.Database.EntityConfigurations;

internal sealed class WordDistanceMapConfiguration
    : IEntityTypeConfiguration<WordDistanceMap>
{
    public void Configure(EntityTypeBuilder<WordDistanceMap> builder)
    {
        builder.ToTable("word_distance_maps");

        builder.HasKey(ws => new { ws.Version, ws.WordId })
            .HasName("pk_word_distance_maps");

        builder.Property(ws => ws.Version)
            .HasColumnName("version")
            .ValueGeneratedNever();

        builder.Property(ws => ws.WordId)
            .HasColumnName("word_id")
            .ValueGeneratedNever();

        builder.Property(ws => ws.WordsIdsByDistance)
            .HasColumnName("words_ids_by_distance");

        builder.Property(ws => ws.DistancesByWordsIds)
            .HasColumnName("distances_by_words_ids");
    }
}
