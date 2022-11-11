using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MSiccDev.ServerlessBlog.EntityModel
{
    public class Blog : IEquatable<Blog>
    {
        public Guid BlogId { get; set; }

        public string Name { get; set; }

        public string Slogan { get; set; }

        public Uri LogoUrl { get; set; }

        public ICollection<Post> Posts { get; set; }

        public ICollection<Author> Authors { get; set; }

        public ICollection<Tag> Tags { get; set; }

        public ICollection<Medium> Media { get; set; }

        public bool Equals(Blog other)
        {
            if (other == null)
                return false;

            return
                this.BlogId == other.BlogId &&
                this.Name == other.Name &&
                this.Slogan == other.Slogan &&
                this.LogoUrl == other.LogoUrl;
        }

        public override bool Equals(object obj)
            => obj is Blog other && Equals(other);

        public static bool operator ==(in Blog self, in Blog other)
            => Equals(self, other);

        public static bool operator !=(in Blog self, in Blog other)
            => !Equals(self, other);

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(this.BlogId);
            hash.Add(this.Name);
            hash.Add(this.Slogan);
            hash.Add(this.LogoUrl);

            return hash.ToHashCode();
        }
    }
}

