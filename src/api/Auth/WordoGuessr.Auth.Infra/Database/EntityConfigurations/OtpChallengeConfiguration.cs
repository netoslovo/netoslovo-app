using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordoGuessr.Auth.Domain;

namespace WordoGuessr.Auth.Infra.Database.EntityConfigurations;

internal sealed class OtpChallengeConfiguration : IEntityTypeConfiguration<OtpChallenge>
{
    public void Configure(EntityTypeBuilder<OtpChallenge> builder)
    {
        builder.ToTable("otp_challenges");

        builder.HasKey(challenge => challenge.Id);

        builder.Property(challenge => challenge.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(challenge => challenge.Email)
            .IsRequired()
            .HasColumnName("email");

        builder.Property(challenge => challenge.CodeHash)
            .IsRequired()
            .HasColumnName("code_hash");

        builder.Property(challenge => challenge.RequestedByGuestId)
            .HasColumnName("requested_by_guest_id");

        builder.Property(challenge => challenge.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(challenge => challenge.Ttl)
            .IsRequired()
            .HasColumnName("ttl");

        builder.Property(challenge => challenge.ConsumedAt)
            .HasColumnName("consumed_at");

        builder.Property(challenge => challenge.AttemptCount)
            .IsRequired()
            .HasColumnName("attempt_count");

        builder.Property<uint>("version")
            .IsRowVersion();
    }
}
