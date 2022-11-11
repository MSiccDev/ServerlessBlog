using System;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSiccDev.ServerlessBlog.EntityModel;

namespace MSiccDev.ServerlessBlog.EFCore.Configurations
{
    public class MediumConfiguration : IEntityTypeConfiguration<Medium>
    {
        public void Configure(EntityTypeBuilder<Medium> builder)
        {
            builder.HasKey(medium => medium.MediumId).
                    HasName($"PK_{nameof(MSiccDev.ServerlessBlog.EntityModel.Medium.MediumId)}");

            builder.Property(nameof(MSiccDev.ServerlessBlog.EntityModel.Medium.MediumId)).
                    ValueGeneratedOnAdd();

            builder.Property(nameof(MSiccDev.ServerlessBlog.EntityModel.Medium.MediumId)).
                    IsRequired();

            builder.Property(nameof(Medium.MediumTypeId)).
                IsRequired();

            builder.Property(nameof(Medium.Description)).
                HasMaxLength(500);

            builder.Property(nameof(Medium.AlternativeText)).
                HasMaxLength(100);

            builder.HasOne(medium => medium.Blog).
                WithMany(blog => blog.Media).
                HasForeignKey(medium => medium.BlogId).
                HasConstraintName($"FK_{nameof(Medium)}_{nameof(Blog)}");

            builder.HasOne(medium => medium.MediumType).
                WithMany(mediumType => mediumType.Media).
                HasForeignKey(medium => medium.MediumTypeId).
                HasConstraintName($"FK_{nameof(Medium)}_{nameof(MediumType)}");
        }
    }
}

