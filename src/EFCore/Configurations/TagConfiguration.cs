using System;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSiccDev.ServerlessBlog.EntityModel;

namespace MSiccDev.ServerlessBlog.EFCore.Configurations
{
    public class TagConfiguration : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            builder.HasKey(tag => tag.TagId).
                    HasName($"PK_{nameof(Tag.TagId)}");

            builder.HasAlternateKey(tag => tag.Name).
                HasName($"AK_{nameof(Tag)}_{nameof(Tag.Name)}");

            builder.HasAlternateKey(tag => tag.Slug).
                HasName($"AK_{nameof(Tag)}_{nameof(Tag.Slug)}");

            builder.Property(nameof(Tag.TagId)).
                    ValueGeneratedOnAdd();

            builder.Property(nameof(Tag.Name)).
                    HasMaxLength(100).
                    IsRequired();

            builder.Property(nameof(Tag.Slug)).
                    HasMaxLength(100).
                    IsRequired();

            builder.HasOne(tag => tag.Blog).
                WithMany(blog => blog.Tags).
                HasForeignKey(tag => tag.BlogId).
                HasConstraintName($"FK_{nameof(Tag)}_{nameof(Blog)}");
        }
    }
}

