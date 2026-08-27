using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WordoGuessr.Auth.Infra.Database.EntityConfigurations;

internal sealed class IdentityUserClaimConfiguration : IEntityTypeConfiguration<IdentityUserClaim<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserClaim<Guid>> builder)
    {
        builder.ToTable("user_claims");

        builder.Property(claim => claim.Id)
            .HasColumnName("id");

        builder.Property(claim => claim.UserId)
            .HasColumnName("user_id");

        builder.Property(claim => claim.ClaimType)
            .HasColumnName("claim_type");

        builder.Property(claim => claim.ClaimValue)
            .HasColumnName("claim_value");
    }
}
