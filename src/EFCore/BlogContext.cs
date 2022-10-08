using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MSiccDev.ServerlessBlog.EFCore.Configurations;
using MSiccDev.ServerlessBlog.EntityModel;

namespace MSiccDev.ServerlessBlog.EFCore
{
    public sealed class BlogContext : DbContext
    {
        public BlogContext(DbContextOptions<BlogContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new BlogConfiguration());
            modelBuilder.ApplyConfiguration(new MediumypeConfiguration());
            modelBuilder.ApplyConfiguration(new MediumConfiguration());
            modelBuilder.ApplyConfiguration(new AuthorConfiguration());
            modelBuilder.ApplyConfiguration(new TagConfiguration());
            modelBuilder.ApplyConfiguration(new PostConfiguration());

            modelBuilder.Seed();
        }


        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<MSiccDev.ServerlessBlog.EntityModel.Medium> Media { get; set; }
        public DbSet<MediumType> MediaTypes { get; set; }
        public DbSet<Tag> Tags { get; set; }
    }
}

