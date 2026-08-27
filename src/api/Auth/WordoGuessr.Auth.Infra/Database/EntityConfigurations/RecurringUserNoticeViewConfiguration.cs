using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordoGuessr.Auth.Domain;

internal sealed class RecurringUserNoticeViewConfiguration
    : IEntityTypeConfiguration<RecurringUserNoticeView>
{
    public void Configure(EntityTypeBuilder<RecurringUserNoticeView> builder)
    {
        builder.Property(view => view.DoNotShowAgain)
            .HasColumnName("do_not_show_again");
    }
}