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
        public Guid? UserImageId { get; set; }

        public List<Post> Posts { get; set; }

        public Blog Blog { get; set; }
        public Guid BlogId { get; set; }


        public bool Equals(Author other)
        {
            if (other == null)
                return false;

            return
                this.AuthorId == other.AuthorId &&
                this.DisplayName == other.DisplayName &&
                this.UserName == other.UserName &&
                this.UserImage == other.UserImage;
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

            return hash.ToHashCode();
        }
    }
}