using System;
using System.Collections.Generic;

namespace MSiccDev.ServerlessBlog.Model
{
    public class Media : IEquatable<Media>
    {
        public Guid MediaId { get; set; }

        public Uri MediaUrl { get; set; }

        public string AlternativeText { get; set; }

        public string Description { get; set; }

        public Guid MediaTypeId { get; set; }
        public MediaType MediaType { get; set; }

        public Guid BlogId { get; set; }
        public Blog Blog { get; set; }

        public bool Equals(Media other)
        {
            if (other == null)
                return false;

            return
                this.MediaId == other.MediaId &&
                this.MediaUrl == other.MediaUrl &&
                this.AlternativeText == other.AlternativeText &&
                this.Description == other.Description &&
                this.BlogId == other.BlogId &&
                this.MediaTypeId == other.MediaTypeId;
        }

        public override bool Equals(object obj)
            => obj is Media other && Equals(other);

        public static bool operator ==(in Media self, in Media other)
            => Equals(self, other);

        public static bool operator !=(in Media self, in Media other)
            => !Equals(self, other);

        public override int GetHashCode()
            => HashCode.Combine(this.MediaId, this.MediaUrl, this.AlternativeText, this.Description, this.BlogId, this.MediaTypeId);
    }
}

