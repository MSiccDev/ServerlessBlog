using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSiccDev.ServerlessBlog.EntityModel;

namespace MSiccDev.ServerlessBlog.EFCore.Configurations
{
    public class PostConfiguration : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            builder.HasKey(post => post.PostId).
                    HasName($"PK_{nameof(Post.PostId)}");

            builder.HasAlternateKey(post => post.Slug).
                    HasName($"AK_{nameof(Post.Slug)}");

            builder.Property(nameof(Post.PostId)).
                    ValueGeneratedOnAdd();

            builder.Property(nameof(Post.Title)).
                    HasMaxLength(255).
                    IsRequired();

            //implicit NVARCHAR(MAX)
            builder.Property(nameof(Post.Content)).
                    IsRequired();

            builder.Property(nameof(Post.Published)).
                    HasDefaultValue(default);

            builder.Property(nameof(Post.LastModified)).
                    HasDefaultValueSql("GETDATE()");

            builder.Property(nameof(Post.Slug)).
                    HasMaxLength(100).
                    IsRequired();

            builder.HasOne(post => post.Blog).
                    WithMany(blog => blog.Posts).
                    HasForeignKey(post => post.BlogId).
                    HasConstraintName($"FK_{nameof(Post)}_{nameof(Blog)}");

            builder.HasOne(post => post.Author).
                    WithMany(author => author.Posts).
                    HasForeignKey(post => post.AuthorId).
                    HasConstraintName($"FK_{nameof(Post)}_{nameof(Author)}").
                    OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(post => post.Tags).
                    WithMany(tag => tag.Posts).
                    UsingEntity<PostTagMapping>(
                    join => join.
                        HasOne(mapping => mapping.Tag).
                        WithMany(tag => tag.PostTagMappings).
                        HasForeignKey(mapping => mapping.TagId).
                        HasConstraintName($"FK_{nameof(PostTagMapping)}_{nameof(Post.Tags)}_{nameof(PostTagMapping.TagId)}").
                        OnDelete(DeleteBehavior.Cascade),
                    join => join.
                        HasOne(mapping => mapping.Post).
                        WithMany(tag => tag.PostTagMappings).
                        HasForeignKey(mapping => mapping.PostId).
                        HasConstraintName($"FK_{nameof(PostTagMapping)}_{nameof(Tag.Posts)}_{nameof(PostTagMapping.PostId)}").
                        OnDelete(DeleteBehavior.ClientCascade),
                    join =>
                    {
                        join.HasKey(mapping => new { mapping.PostId, mapping.TagId });
                    });

            builder.HasMany(post => post.Media).
                    WithMany(medium => medium.Posts).
                    UsingEntity<PostMediumMapping>(
                    join => join.
                        HasOne(mapping => mapping.Medium).
                        WithMany(medium => medium.PostMediumMappings).
                        HasForeignKey(mapping => mapping.MediumId).
                        HasConstraintName($"FK_{nameof(PostMediumMapping)}_{nameof(Post.Media)}_{nameof(PostMediumMapping.MediumId)}").
                        OnDelete(DeleteBehavior.Cascade),
                    join => join.
                        HasOne(mapping => mapping.Post).
                        WithMany(medium => medium.PostMediumMappings).
                        HasForeignKey(mapping => mapping.PostId).
                        HasConstraintName($"FK_{nameof(PostMediumMapping)}_{nameof(Medium.Posts)}_{nameof(PostMediumMapping.PostId)}").
                        OnDelete(DeleteBehavior.ClientCascade),
                    join =>
                    {
                        join.HasKey(mapping => new { mapping.PostId, mapping.MediumId });
                    });

        }
    }
}

