using System;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSiccDev.ServerlessBlog.Model;

namespace MSiccDev.ServerlessBlog.EFCore.Configurations
{
    public class MediaTypeConfiguration : IEntityTypeConfiguration<MediaType>
    {
        public void Configure(EntityTypeBuilder<MediaType> builder)
        {
            builder.HasKey(type => type.MediaTypeId).
                HasName($"PK_{nameof(MediaType.MediaTypeId)}");

            builder.HasAlternateKey(type => type.MimeType).
                    HasName($"AK_{nameof(MediaType.MimeType)}");

            builder.Property(nameof(MediaType.MediaTypeId)).
                    ValueGeneratedOnAdd();

            builder.Property(nameof(MediaType.Encoding)).
                    HasMaxLength(20);

            builder.Property(nameof(MediaType.Name)).
                    HasMaxLength(100).
                    IsRequired();

            builder.Property(nameof(MediaType.MimeType)).
                    HasMaxLength(100).
                    IsRequired();
        }
    }
}

