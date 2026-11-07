using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WordoGuessr.WebAPI.DataProtection;

internal sealed class DataProtectionKeyConfiguration : IEntityTypeConfiguration<DataProtectionKey>
{
    public void Configure(EntityTypeBuilder<DataProtectionKey> builder)
    {
        builder.ToTable("keys");

        builder.Property(keys => keys.Id)
            .HasColumnName("id");

        builder.Property(keys => keys.FriendlyName)
            .HasColumnName("friendly_name");

        builder.Property(keys => keys.Xml)
            .HasColumnName("xml");
    }
}