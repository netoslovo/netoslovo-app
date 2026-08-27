using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordoGuessr.Words.App.Abstractions;

namespace WordoGuessr.Words.Infra.Database.EntityConfigurations;

internal sealed class WordIndexConfiguration
    : IEntityTypeConfiguration<WordIndex>
{
    public void Configure(EntityTypeBuilder<WordIndex> builder)
    {
        builder.ToTable("words_indexes");

        builder.HasKey(wi => new { wi.Version, wi.WordId })
            .HasName("pk_words_indexes");

        builder.Property(wi => wi.Version)
            .HasColumnName("version")
            .ValueGeneratedNever();

        builder.Property(wi => wi.WordId)
            .HasColumnName("word_id")
            .ValueGeneratedNever();

        builder.Property(wi => wi.WordText)
            .HasColumnName("word_text");
    }
}
