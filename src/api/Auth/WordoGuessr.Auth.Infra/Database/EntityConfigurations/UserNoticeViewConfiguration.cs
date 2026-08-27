using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WordoGuessr.Auth.Domain;

internal sealed class UserNoticeViewConfiguration
    : IEntityTypeConfiguration<UserNoticeView>
{
    public void Configure(EntityTypeBuilder<UserNoticeView> builder)
    {
        builder.ToTable("user_notice_views");

        builder.HasKey(view => new
        {
            view.UserId,
            view.NoticeCode
        });

        builder.Property(view => view.UserId)
            .IsRequired()
            .HasColumnName("user_id");

        builder.Property(view => view.NoticeCode)
            .IsRequired()
            .HasColumnName("notice_code");

        builder.HasDiscriminator<UserNoticeViewType>("type")
            .HasValue<OneTimeUserNoticeView>(UserNoticeViewType.OneTime)
            .HasValue<RecurringUserNoticeView>(UserNoticeViewType.Recurring);

        builder.Property<UserNoticeViewType>("type")
            .HasColumnName("type");

        builder.Property<uint>("version")
            .IsRowVersion();
    }
}

internal enum UserNoticeViewType
{
    OneTime,
    Recurring
}