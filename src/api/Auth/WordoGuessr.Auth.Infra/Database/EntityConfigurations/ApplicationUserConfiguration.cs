using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordoGuessr.Auth.Domain;

namespace WordoGuessr.Auth.Infra.Database.EntityConfigurations;

internal sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("users");

        builder.Property(user => user.Id)
            .HasColumnName("id");

        builder.Property(user => user.UserName)
            .HasColumnName("user_name");

        builder.Property(user => user.NormalizedUserName)
            .HasColumnName("normalized_user_name");

        builder.Property(user => user.Email)
            .HasColumnName("email");

        builder.Property(user => user.NormalizedEmail)
            .HasColumnName("normalized_email");

        builder.Property(user => user.EmailConfirmed)
            .HasColumnName("email_confirmed");

        builder.Property(user => user.PasswordHash)
            .HasColumnName("password_hash");

        builder.Property(user => user.SecurityStamp)
            .HasColumnName("security_stamp");

        builder.Property(user => user.ConcurrencyStamp)
            .HasColumnName("concurrency_stamp");

        builder.Property(user => user.PhoneNumber)
            .HasColumnName("phone_number");

        builder.Property(user => user.PhoneNumberConfirmed)
            .HasColumnName("phone_number_confirmed");

        builder.Property(user => user.TwoFactorEnabled)
            .HasColumnName("two_factor_enabled");

        builder.Property(user => user.LockoutEnd)
            .HasColumnName("lockout_end");

        builder.Property(user => user.LockoutEnabled)
            .HasColumnName("lockout_enabled");

        builder.Property(user => user.AccessFailedCount)
            .HasColumnName("access_failed_count");

        builder.Property(user => user.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(user => user.LastLoginAt)
            .HasColumnName("last_login_at");

        builder.Property(user => user.UserNameChangedAt)
            .HasColumnName("user_name_changed_at");
    }
}
