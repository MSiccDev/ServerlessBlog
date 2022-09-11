﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;

namespace MSiccDev.ServerlessBlog.Model
{
    public class Post : IEquatable<Post>
    {
        public Guid PostId { get; set; }

        public string Title { get; set; }
        public string Content { get; set; }

        public string Slug { get; set; }

        public DateTimeOffset Published { get; set; }
        public DateTimeOffset LastModified { get; set; }

        public Media PostImage { get; set; }
        public Guid PostImageMediaId { get; set; }

        public Author Author { get; set; }
        public Guid AuthorId { get; set; }

        public Guid BlogId { get; set; }
        public Blog Blog { get; set; }


        public List<Tag> Tags { get; set; } = new List<Tag>();

        public List<Media> Media { get; set; } = new List<Media>();

        public List<PostMediaMapping> PostMediaMappings { get; set; }
        public List<PostTagMapping> PostTagMappings { get; set; }

        public bool Equals(Post other)
        {
            if (other == null)
                return false;

            return
                this.Title == other.Title &&
                this.Content == other.Content &&
                this.PostImage == other.PostImage &&
                this.PostImageMediaId == other.PostImageMediaId &&
                this.Slug == other.Slug &&
                this.BlogId == other.BlogId &&
                this.Blog == other.Blog &&
                this.Author == other.Author &&
                this.AuthorId == other.AuthorId &&
                this.Published == other.Published &&
                this.LastModified == other.LastModified &&
                this.Media.Except(other.Media).Count() == 0 &&
                this.Tags.Except(other.Tags).Count() == 0;
        }

        public override bool Equals(object obj)
            => obj is Post other && Equals(other);

        public static bool operator ==(in Post self, in Post other)
            => Equals(self, other);

        public static bool operator !=(in Post self, in Post other)
            => !Equals(self, other);

        public override int GetHashCode()
        {
            //https://docs.microsoft.com/en-us/dotnet/api/system.hashcode?view=net-6.0
            HashCode hash = new HashCode();
            hash.Add(this.BlogId);
            hash.Add(this.Blog);
            hash.Add(this.PostId);
            hash.Add(this.AuthorId);
            hash.Add(this.Author);
            hash.Add(this.PostImageMediaId);
            hash.Add(this.Title);
            hash.Add(this.Content);
            hash.Add(this.Slug);
            hash.Add(this.Published);
            hash.Add(this.LastModified);

            for (int index = 0; index < this.Media.Count; index++)
                hash.Add(this.Media[index]);

            for (int index = 0; index < this.Tags.Count; index++)
                hash.Add(this.Tags[index]);

            return hash.ToHashCode();
        }
    }
}