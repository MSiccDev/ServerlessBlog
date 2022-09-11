using System;
using System.Collections.Generic;

namespace MSiccDev.ServerlessBlog.Model
{
	public class Tag : IEquatable<Tag>
	{
		public Guid TagId { get; set; }

		public string Name { get; set; }

		public string Slug { get; set; }

		public List<Blog> Blogs { get; set; }

		public List<Post> Posts { get; set; }

		public List<PostTagMapping> PostTagMappings { get; set; }



		public bool Equals(Tag other)
		{
			if (other == null)
				return false;

			return
				this.TagId == other.TagId &&
				this.Name == other.Name &&
				this.Slug == other.Slug;
		}

		public override bool Equals(object obj)
			=> obj is Tag other && Equals(other);

		public static bool operator ==(in Tag self, in Tag other)
			=> Equals(self, other);

		public static bool operator !=(in Tag self, in Tag other)
			=> !Equals(self, other);


		public override int GetHashCode() =>
			HashCode.Combine(this.TagId, this.Name, this.Slug);
	}
}