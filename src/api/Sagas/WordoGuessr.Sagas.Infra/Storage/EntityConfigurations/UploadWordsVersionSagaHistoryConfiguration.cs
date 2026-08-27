using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordoGuessr.Sagas.Domain.UploadWordsVersion;

namespace WordoGuessr.Sagas.Infra.Storage.EntityConfigurations;

internal sealed class UploadWordsVersionSagaHistoryConfiguration
    : IEntityTypeConfiguration<UploadWordsVersionSagaHistory>
{
    public void Configure(EntityTypeBuilder<UploadWordsVersionSagaHistory> builder)
    {
        builder.ToTable("upload_words_version_sagas_history");

        builder.HasKey(saga => saga.SagaId);

        builder.Property(saga => saga.SagaId)
            .HasColumnName("saga_id");

        builder.Property(saga => saga.WordsVersion)
            .HasColumnName("words_version");

        builder.Property(saga => saga.State)
            .HasColumnName("state");

        builder.Property(saga => saga.CurrentStep)
            .HasColumnName("current_step");

        builder.Property(saga => saga.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(saga => saga.CompletedAt)
            .HasColumnName("completed_at");

        builder.ComplexCollection(saga => saga.CompletedSteps, steps =>
        {
            steps.ToJson("completed_steps");

            steps.HasField("_completedSteps");
            steps.UsePropertyAccessMode(PropertyAccessMode.Field);

            steps.Property(step => step.Name)
                .HasJsonPropertyName("name")
                .HasConversion<string>();

            steps.Property(step => step.IsSuccess)
                .HasJsonPropertyName("isSuccess");

            steps.Property(step => step.CompletedAt)
                .HasJsonPropertyName("completedAt");
        });

        builder.Property(saga => saga.Version)
            .HasColumnName("version")
            .IsConcurrencyToken();
    }
}
