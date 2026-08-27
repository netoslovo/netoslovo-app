using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WordoGuessr.Auth.Infra.Database.EntityConfigurations;

internal sealed class IdentityUserTokenConfiguration : IEntityTypeConfiguration<IdentityUserToken<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserToken<Guid>> builder)
    {
        builder.ToTable("user_tokens");

        builder.Property(token => token.UserId)
            .HasColumnName("user_id");

        builder.Property(token => token.LoginProvider)
            .HasColumnName("login_provider");

        builder.Property(token => token.Name)
            .HasColumnName("name");

        builder.Property(token => token.Value)
            .HasColumnName("value");
    }
}
