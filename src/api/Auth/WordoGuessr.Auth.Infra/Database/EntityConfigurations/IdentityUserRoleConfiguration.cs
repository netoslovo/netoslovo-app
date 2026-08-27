using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WordoGuessr.Auth.Infra.Database.EntityConfigurations;

internal sealed class IdentityUserRoleConfiguration : IEntityTypeConfiguration<IdentityUserRole<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserRole<Guid>> builder)
    {
        builder.ToTable("user_roles");

        builder.Property(claim => claim.UserId)
            .HasColumnName("user_id");

        builder.Property(claim => claim.RoleId)
            .HasColumnName("role_id");
    }
}
