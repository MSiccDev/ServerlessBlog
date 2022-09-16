using System;
using System.Collections.Generic;
using System.Linq;

namespace MSiccDev.ServerlessBlog.Model
{
    public class Author : IEquatable<Author>
    {
        public Guid AuthorId { get; set; }

        public string DisplayName { get; set; }

        public string UserName { get; set; }

        public Media UserImage { get; set; }
        public Guid UserImageId { get; set; }

        public List<Post> Posts { get; set; }

        public List<Blog> Blogs { get; set; }


        public bool Equals(Author other)
        {
            if (other == null)
                return false;

            return
                this.AuthorId == other.AuthorId &&
                this.DisplayName == other.DisplayName &&
                this.UserName == other.UserName &&
                this.UserImage == other.UserImage &&
                this.Posts.Except(other.Posts).Count() == 0 &&
                this.Blogs.Except(other.Blogs).Count() == 0;
        }

        public override bool Equals(object obj)
            => obj is Author other && Equals(other);

        public static bool operator ==(in Author self, in Author other)
            => Equals(self, other);

        public static bool operator !=(in Author self, in Author other)
            => !Equals(self, other);


        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(this.AuthorId);
            hash.Add(this.DisplayName);
            hash.Add(this.UserName);
            hash.Add(this.UserImage);

            for (int index = 0; index < this.Blogs.Count; index++)
                hash.Add(this.Blogs[index]);

            for (int index = 0; index < this.Posts.Count; index++)
                hash.Add(this.Posts[index]);

            return hash.ToHashCode();
        }
    }
}