using System;
using System.Collections.Generic;

namespace MSiccDev.ServerlessBlog.EntityModel
{
    public class MediumType : IEquatable<MediumType>
    {
        public Guid MediumTypeId { get; set; }

        public string MimeType { get; set; }

        public string Name { get; set; }

        public string Encoding { get; set; }

        public ICollection<Medium> Media { get; set; }

        public bool Equals(MediumType other)
        {
            if (other == null)
                return false;

            return
                this.MediumTypeId == other.MediumTypeId &&
                this.MimeType == other.MimeType &&
                this.Name == other.Name &&
                this.Encoding == other.Encoding;
        }

        public override bool Equals(object obj)
            => obj is MediumType other && Equals(other);

        public static bool operator ==(in MediumType self, in MediumType other)
            => Equals(self, other);

        public static bool operator !=(in MediumType self, in MediumType other)
            => !Equals(self, other);

        public override int GetHashCode()
            => HashCode.Combine(this.MediumTypeId, this.MimeType, this.Name, this.Encoding);

    }
}

