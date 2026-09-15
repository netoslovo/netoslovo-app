using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordoGuessr.Game.Domain;

namespace WordoGuessr.Game.Infra.EntityConfigurations;

public sealed class SingleGameConfiguration : IEntityTypeConfiguration<SingleGame>
{
    public void Configure(EntityTypeBuilder<SingleGame> builder)
    {
        builder.ToTable("single_games");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Id)
            .HasColumnName("id");

        builder.HasDiscriminator<SingleGameMode>("_mode")
            .HasValue<DailySingleGame>(SingleGameMode.Daily)
            .HasValue<ArcadeSingleGame>(SingleGameMode.Arcade);

        builder.Property<SingleGameMode>("_mode")
            .HasColumnName("mode")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasOne(g => g.VersionedGameSource)
            .WithMany()
            .HasForeignKey("game_source_id", "words_version");

        builder.Property(g => g.PlayerId)
            .HasColumnName("player_id");

        builder.Property(g => g.StateCode)
            .HasColumnName("state");

        builder.Property(g => g.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(g => g.StartedAt)
            .HasColumnName("started_at");

        builder.Property(g => g.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(g => g.FinishedAt)
            .HasColumnName("finished_at");

        builder.Property(g => g.ClosestGuessDistance)
            .HasColumnName("closest_guess_distance");

        builder.HasMany(g => g.Guesses)
            .WithOne()
            .HasForeignKey("game_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(g => g.Guesses)
            .HasField("_guesses")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property<int>("_revealLetterHintsTotal")
            .HasColumnName("reveal_letter_hints_total")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property<int[]?>("_calculatedRevealLetterHintPenalties")
            .HasColumnName("calculated_reveal_letter_hint_penalties")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property<int[]?>("_orderedLettersIndexesForReveal")
            .HasColumnName("ordered_letters_indexes_for_reveal")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsOne<DisplayWord>("_displayWord", displayWord =>
        {
            displayWord.ToJson("display_word");

            displayWord.Property("_word")
                .HasJsonPropertyName("word")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            displayWord
                .OwnsMany<DisplayWordCell>("_cells", cells =>
                {
                    cells.HasJsonPropertyName("cells");

                    cells.Property<char>("_internalValue")
                        .HasJsonPropertyName("value");

                    cells.Ignore(x => x.Value);

                    cells.Property(x => x.Revealed)
                        .HasJsonPropertyName("revealed");
                });
        });

        builder.ComplexCollection(g => g.UsedHints, hints =>
        {
            hints.ToJson("used_hints");

            hints.HasField("_usedHints");
            hints.UsePropertyAccessMode(PropertyAccessMode.Field);

            hints.Property(h => h.Type)
                .HasJsonPropertyName("type")
                .HasConversion<string>();

            hints.Property(h => h.UsedAt)
                .HasJsonPropertyName("usedAt");
        });

        builder.Property<uint>("version")
            .IsRowVersion();

        builder.Ignore(g => g.HalfwayWordHintsLeft);
        builder.Ignore(g => g.RevealLetterHintsLeft);
        builder.Ignore(g => g.RevealLetterHintsTotal);
    }
}
