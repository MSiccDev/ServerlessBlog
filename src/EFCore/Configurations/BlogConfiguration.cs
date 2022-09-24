using System;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSiccDev.ServerlessBlog.EntityModel;

namespace MSiccDev.ServerlessBlog.EFCore.Configurations
{
    public class BlogConfiguration : IEntityTypeConfiguration<Blog>
    {
        public void Configure(EntityTypeBuilder<Blog> builder)
        {
            builder.Property(nameof(Blog.BlogId)).
                IsRequired();

            builder.Property(nameof(Blog.BlogId)).
                ValueGeneratedOnAdd();

            builder.HasKey(blog => blog.BlogId).
                HasName($"PK_{nameof(Blog.BlogId)}");

            builder.Property(nameof(Blog.Name)).
                HasMaxLength(255).
                IsRequired();

            builder.Property(nameof(Blog.Slogan)).
                HasMaxLength(255).
                IsRequired();

            builder.Property(nameof(Blog.LogoUrl)).
                IsRequired();
        }
    }
}

