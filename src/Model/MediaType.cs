using System;
namespace MSiccDev.ServerlessBlog.Model
{
	public class MediaType : IEquatable<MediaType>
	{
		public Guid MediaTypeId { get; set; }

		public string MimeType { get; set; }

		public string Name { get; set; }

		public string Encoding { get; set; }



		public bool Equals(MediaType other)
		{
			if (other == null)
				return false;

			return
				this.MediaTypeId == other.MediaTypeId &&
				this.MimeType == other.MimeType &&
				this.Name == other.Name &&
				this.Encoding == other.Encoding;
		}

		public override bool Equals(object obj)
			=> obj is MediaType other && Equals(other);

		public static bool operator ==(in MediaType self, in MediaType other)
			=> Equals(self, other);

		public static bool operator !=(in MediaType self, in MediaType other)
			=> !Equals(self, other);

		public override int GetHashCode()
			=> HashCode.Combine(this.MediaTypeId, this.MimeType, this.Name, this.Encoding);

	}
}

