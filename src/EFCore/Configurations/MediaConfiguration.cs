using System;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSiccDev.ServerlessBlog.EntityModel;

namespace MSiccDev.ServerlessBlog.EFCore.Configurations
{
    public class MediaConfiguration : IEntityTypeConfiguration<Media>
    {
        public void Configure(EntityTypeBuilder<Media> builder)
        {
            builder.HasKey(media => media.MediaId).
                    HasName($"PK_{nameof(MSiccDev.ServerlessBlog.EntityModel.Media.MediaId)}");

            builder.Property(nameof(MSiccDev.ServerlessBlog.EntityModel.Media.MediaId)).
                    ValueGeneratedOnAdd();

            builder.Property(nameof(MSiccDev.ServerlessBlog.EntityModel.Media.MediaUrl)).
                    IsRequired();

            builder.Property(nameof(Media.MediaTypeId)).
                IsRequired();

            builder.Property(nameof(Media.Description)).
                HasMaxLength(500);

            builder.Property(nameof(Media.AlternativeText)).
                HasMaxLength(100);

            builder.HasOne(media => media.Blog).
                WithMany(blog => blog.Media).
                HasForeignKey(media => media.BlogId).
                HasConstraintName($"FK_{nameof(Media)}_{nameof(Blog)}");
        }
    }
}

