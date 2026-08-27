using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordoGuessr.Auth.Domain;

namespace WordoGuessr.Auth.Infra.Database.EntityConfigurations;

internal sealed class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.ToTable("roles");

        builder.Property(role => role.Id)
            .HasColumnName("id");

        builder.Property(role => role.Name)
            .HasColumnName("name");

        builder.Property(role => role.NormalizedName)
            .HasColumnName("normalized_name");

        builder.Property(role => role.ConcurrencyStamp)
            .HasColumnName("concurrency_stamp");
    }
}
