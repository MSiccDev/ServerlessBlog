using System;
using System.Collections.Generic;

namespace MSiccDev.ServerlessBlog.EntityModel
{
    public class Medium : IEquatable<Medium>
    {
        public Guid MediumId { get; set; }

        public Uri MediumUrl { get; set; }

        public string AlternativeText { get; set; }

        public string Description { get; set; }

        public Guid MediumTypeId { get; set; }
        public MediumType MediumType { get; set; }

        public Guid BlogId { get; set; }
        public Blog Blog { get; set; }

        public ICollection<Post> Posts { get; set; }
        public ICollection<Author> Authors { get; set; }

        public List<PostMediumMapping> PostMediumMappings { get; set; }

        public bool Equals(Medium other)
        {
            if (other == null)
                return false;

            return
                this.MediumId == other.MediumId &&
                this.MediumUrl == other.MediumUrl &&
                this.AlternativeText == other.AlternativeText &&
                this.Description == other.Description &&
                this.BlogId == other.BlogId &&
                this.MediumTypeId == other.MediumTypeId;
        }

        public override bool Equals(object obj)
            => obj is Medium other && Equals(other);

        public static bool operator ==(in Medium self, in Medium other)
            => Equals(self, other);

        public static bool operator !=(in Medium self, in Medium other)
            => !Equals(self, other);

        public override int GetHashCode()
            => HashCode.Combine(this.MediumId, this.MediumUrl, this.AlternativeText, this.Description, this.BlogId, this.MediumTypeId);
    }
}

