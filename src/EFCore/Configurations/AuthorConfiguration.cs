using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSiccDev.ServerlessBlog.EntityModel;

namespace MSiccDev.ServerlessBlog.EFCore.Configurations
{
    public class AuthorConfiguration : IEntityTypeConfiguration<Author>
    {
        public void Configure(EntityTypeBuilder<Author> builder)
        {
            builder.HasKey(author => author.AuthorId).
                    HasName($"PK_{nameof(Author.AuthorId)}");

            builder.Property(nameof(Author.AuthorId)).
                    ValueGeneratedOnAdd();

            builder.Property(nameof(Author.DisplayName)).
                    HasMaxLength(255).
                    IsRequired();

            builder.Property(nameof(Author.UserName)).
                    HasMaxLength(255).
                    IsRequired();

            builder.Property(nameof(Author.UserImageId)).
                    IsRequired(false);

            builder.HasOne(author => author.Blog).
                    WithMany(blog => blog.Authors).
                    HasForeignKey(author => author.BlogId).
                    HasConstraintName($"FK_{nameof(Author)}_{nameof(Blog)}");

        }
    }
}

