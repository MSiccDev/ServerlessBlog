using System;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSiccDev.ServerlessBlog.EntityModel;

namespace MSiccDev.ServerlessBlog.EFCore.Configurations
{
    public class MediumypeConfiguration : IEntityTypeConfiguration<MediumType>
    {
        public void Configure(EntityTypeBuilder<MediumType> builder)
        {
            builder.HasKey(type => type.MediumTypeId).
                HasName($"PK_{nameof(MediumType.MediumTypeId)}");

            builder.HasAlternateKey(type => type.MimeType).
                    HasName($"AK_{nameof(MediumType.MimeType)}");

            builder.Property(nameof(MediumType.MediumTypeId)).
                    ValueGeneratedOnAdd();

            builder.Property(nameof(MediumType.Encoding)).
                    HasMaxLength(20);

            builder.Property(nameof(MediumType.Name)).
                    HasMaxLength(100).
                    IsRequired();

            builder.Property(nameof(MediumType.MimeType)).
                    HasMaxLength(100).
                    IsRequired();
        }
    }
}

