using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WordoGuessr.Auth.Infra.Database.EntityConfigurations;

internal sealed class IdentityUserLoginConfiguration : IEntityTypeConfiguration<IdentityUserLogin<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserLogin<Guid>> builder)
    {
        builder.ToTable("user_logins");

        builder.Property(login => login.LoginProvider)
            .HasColumnName("login_provider");

        builder.Property(login => login.ProviderKey)
            .HasColumnName("provider_key");

        builder.Property(login => login.ProviderDisplayName)
            .HasColumnName("provider_display_name");

        builder.Property(login => login.UserId)
            .HasColumnName("user_id");
    }
}
