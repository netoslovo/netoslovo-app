using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordoGuessr.Words.App.Abstractions;

namespace WordoGuessr.Words.Infra.Database.EntityConfigurations;

internal sealed class WordsVersionConfiguration
    : IEntityTypeConfiguration<WordsVersion>
{
    public void Configure(EntityTypeBuilder<WordsVersion> builder)
    {
        builder.ToTable("words_versions");

        builder.HasKey(wv => wv.Version);

        builder.Property(wv => wv.Version)
            .HasColumnName("version")
            .ValueGeneratedNever();

        builder.Property(wv => wv.State)
            .HasColumnName("state");

        builder.Property(wv => wv.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(wv => wv.ActivatedAt)
            .HasColumnName("activated_at");

        builder.Property(wv => wv.RetiredAt)
            .HasColumnName("retired_at");
    }
}
