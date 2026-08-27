using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WordoGuessr.Auth.Infra.Database.EntityConfigurations;

internal sealed class IdentityRoleClaimConfiguration : IEntityTypeConfiguration<IdentityRoleClaim<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityRoleClaim<Guid>> builder)
    {
        builder.ToTable("role_claims");

        builder.Property(claim => claim.Id)
            .HasColumnName("id");

        builder.Property(claim => claim.RoleId)
            .HasColumnName("role_id");

        builder.Property(claim => claim.ClaimType)
            .HasColumnName("claim_type");

        builder.Property(claim => claim.ClaimValue)
            .HasColumnName("claim_value");
    }
}
