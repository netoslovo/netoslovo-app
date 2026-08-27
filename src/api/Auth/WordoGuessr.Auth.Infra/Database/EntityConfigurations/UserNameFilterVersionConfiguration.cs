using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordoGuessr.Auth.Infra.Identity.UserNameFiltration;

namespace WordoGuessr.Auth.Infra.Database.EntityConfigurations;

internal sealed class UserNameFilterVersionConfiguration : IEntityTypeConfiguration<UserNameFilterVersion>
{
    public void Configure(EntityTypeBuilder<UserNameFilterVersion> builder)
    {
        builder.ToTable("user_name_filter_versions");

        builder.HasKey(version => version.Id);

        builder.Property(version => version.Id)
            .HasColumnName("id");

        builder.Property(version => version.Version)
            .ValueGeneratedNever()
            .HasColumnName("version");

        builder.Property(version => version.PublishedAt)
            .HasColumnName("published_at");

        builder.Property(version => version.IsActive)
            .HasColumnName("is_active");
    }
}
