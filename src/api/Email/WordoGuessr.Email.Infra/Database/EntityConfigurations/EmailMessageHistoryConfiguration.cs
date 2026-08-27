using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordoGuessr.Email.Infra.EmailSending;

namespace WordoGuessr.Email.Infra.Database.EntityConfigurations;

internal sealed class EmailMessageHistoryConfiguration : IEntityTypeConfiguration<EmailMessageHistory>
{
    public void Configure(EntityTypeBuilder<EmailMessageHistory> builder)
    {
        builder.ToTable("messages_history");

        builder.HasKey(message => message.MessageId);

        builder.Property(message => message.MessageId)
            .ValueGeneratedNever()
            .HasColumnName("message_id");

        builder.Property(message => message.To)
            .HasColumnName("to");

        builder.Property(message => message.Subject)
            .HasColumnName("subject");

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

        builder.Property(message => message.ArchivedAt)
            .HasColumnName("archived_at");

        builder.Property(message => message.ArchiveReason)
            .HasColumnName("archive_reason");
    }
}
