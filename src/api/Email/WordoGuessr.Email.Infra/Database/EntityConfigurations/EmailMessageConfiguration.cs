using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordoGuessr.Email.Domain;

namespace WordoGuessr.Email.Infra.Database.EntityConfigurations;

internal sealed class EmailMessageConfiguration : IEntityTypeConfiguration<EmailMessage>
{
    public void Configure(EntityTypeBuilder<EmailMessage> builder)
    {
        builder.ToTable("messages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(message => message.To)
            .HasColumnName("to");

        builder.Property(message => message.Subject)
            .HasColumnName("subject");

        builder.Property(message => message.Body)
            .HasColumnName("body");

        builder.Property(message => message.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(message => message.ExpireAt)
            .HasColumnName("expire_at");

        builder.Property(message => message.Attempts)
            .HasColumnName("attempts");

        builder.Property(message => message.State)
            .HasColumnName("state");

        builder.Property(message => message.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(message => message.SourceId)
            .HasColumnName("source_id");

        builder.Property(message => message.Version)
            .IsRowVersion();
    }
}
