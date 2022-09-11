using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MSiccDev.ServerlessBlog.Model;

namespace MSiccDev.ServerlessBlog.EFCore
{
	public sealed class BlogContext : DbContext
	{
		public BlogContext(DbContextOptions<BlogContext> options) : base(options)
		{

		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			ConfigureBlogs(modelBuilder);

			ConfigurePosts(modelBuilder);

			ConfigureMedia(modelBuilder);

			ConfigureAuthors(modelBuilder);

			ConfigureTags(modelBuilder);
		}

		private void ConfigureBlogs(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Blog>().
				HasKey(blog => blog.BlogId).HasName($"PK_{nameof(Blog.BlogId)}");

			modelBuilder.Entity<Blog>().
				HasMany(blog => blog.Authors).WithMany(author => author.Blogs).
				UsingEntity(join => join.ToTable($"{nameof(Blog)}{nameof(Author)}Mappings"));

			modelBuilder.Entity<Blog>().
				HasMany(blog => blog.Tags).WithMany(tag => tag.Blogs).
				UsingEntity(join => join.ToTable($"{nameof(Blog)}{nameof(Tags)}Mappings"));
		}

		private void ConfigurePosts(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Post>().
				HasKey(post => post.PostId).HasName($"PK_{nameof(Post.PostId)}");

			modelBuilder.Entity<Post>().
				HasOne(post => post.Blog).WithMany(blog => blog.Posts).
				HasForeignKey(post => post.BlogId).
				HasConstraintName($"FK_{nameof(Post)}_{nameof(Blog)}");

			modelBuilder.Entity<Post>().
				HasOne(post => post.Author).WithMany(author => author.Posts).
				HasForeignKey(post => post.AuthorId).
				HasConstraintName($"FK_{nameof(Post)}_{nameof(Author)}");

			modelBuilder.Entity<Post>().
				HasMany(post => post.Tags).
				WithMany(tag => tag.Posts).
				UsingEntity<PostTagMapping>(
					join => join.
							HasOne(mapping => mapping.Tag).
							WithMany(tag => tag.PostTagMappings).
							HasForeignKey(mapping => mapping.TagId).
							OnDelete(DeleteBehavior.NoAction),
					join => join.
							HasOne(mapping => mapping.Post).
							WithMany(post => post.PostTagMappings).
							HasForeignKey(mapping => mapping.PostId).
							OnDelete(DeleteBehavior.NoAction),
					join => join.HasKey(mapping => new { mapping.PostId, mapping.TagId }));

			modelBuilder.Entity<Post>().
				HasMany(post => post.Media).
				WithMany(media => media.Posts).
				UsingEntity<PostMediaMapping>(
					join => join.
							HasOne(mapping => mapping.Media).
							WithMany(media => media.PostMediaMappings).
							HasForeignKey(mapping => mapping.MediaId).
							OnDelete(DeleteBehavior.NoAction),
					join => join.
							HasOne(mapping => mapping.Post).
							WithMany(media => media.PostMediaMappings).
							HasForeignKey(mapping => mapping.PostId).
							OnDelete(DeleteBehavior.NoAction),
					join => join.HasKey(mapping => new { mapping.PostId, mapping.MediaId }));


			modelBuilder.Entity<Post>().
				HasOne(post => post.PostImage).WithMany().
				HasForeignKey(post => post.PostImageMediaId).
				HasConstraintName($"FK_{nameof(Post.PostImageMediaId)}").
				OnDelete(DeleteBehavior.NoAction);

		}

		private void ConfigureMedia(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Model.Media>().
				HasKey(media => media.MediaId).HasName($"PK_{nameof(MSiccDev.ServerlessBlog.Model.Media.MediaId)}");

			modelBuilder.Entity<MediaType>().
				HasKey(type => type.MimeType).HasName($"PK_{nameof(MediaType.MimeType)}");
		}

		private void ConfigureAuthors(ModelBuilder modelBuilder) =>
				modelBuilder.Entity<Author>().
				HasKey(author => author.AuthorId).HasName($"PK_{nameof(Author.AuthorId)}");

		private void ConfigureTags(ModelBuilder modelBuilder) =>
				modelBuilder.Entity<Tag>().
				HasKey(tag => tag.TagId).HasName($"PK_{nameof(Tag.TagId)}");

		public DbSet<Blog> Blogs { get; set; }
		public DbSet<Post> Posts { get; set; }
		public DbSet<Author> Authors { get; set; }
		public DbSet<Model.Media> Media { get; set; }
		public DbSet<MediaType> MediaTypes { get; set; }
		public DbSet<Tag> Tags { get; set; }
		public DbSet<PostMediaMapping> PostMediaMappings { get; set; }
		public DbSet<PostTagMapping> PostTagMappings { get; set; }

	}
}

