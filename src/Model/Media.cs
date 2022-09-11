using System;
using System.Collections.Generic;

namespace MSiccDev.ServerlessBlog.Model
{
	public class Media : IEquatable<Media>
	{

		public Guid MediaId { get; set; }

		public Uri MediaUrl { get; set; }

		public MediaType MediaType { get; set; }

		public Guid BlogId { get; set; }
		public Blog Blog { get; set; }

		public List<Post> Posts { get; set; }

		public List<PostMediaMapping> PostMediaMappings { get; set; }


		public bool Equals(Media other)
		{
			if (other == null)
				return false;

			return
				this.MediaId == other.MediaId &&
				this.MediaUrl == other.MediaUrl &&
				this.BlogId == other.BlogId &&
				this.Blog == other.Blog &&
				this.MediaType == other.MediaType;
		}

		public override bool Equals(object obj)
			=> obj is Media other && Equals(other);

		public static bool operator ==(in Media self, in Media other)
			=> Equals(self, other);

		public static bool operator !=(in Media self, in Media other)
			=> !Equals(self, other);

		public override int GetHashCode()
			=> HashCode.Combine(this.MediaId, this.MediaUrl, this.BlogId, this.Blog, this.MediaType);
	}
}

