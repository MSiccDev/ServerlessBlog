using System;
using System.Collections.Generic;
using System.Linq;

namespace MSiccDev.ServerlessBlog.Model
{
	public class Blog : IEquatable<Blog>
	{
		public Guid BlogId { get; set; }

		public string Name { get; set; }

		public string Slogan { get; set; }

		public Uri LogoUrl { get; set; }

		public List<Post> Posts { get; set; } = new List<Post>();

		public List<Author> Authors { get; set; } = new List<Author>();

		public List<Tag> Tags { get; set; } = new List<Tag>();


		public bool Equals(Blog other)
		{
			if (other == null)
				return false;

			return
				this.BlogId == other.BlogId &&
				this.Name == other.Name &&
				this.Slogan == other.Slogan &&
				this.LogoUrl == other.LogoUrl &&
				this.Posts.Except(other.Posts).Count() == 0 &&
				this.Authors.Except(other.Authors).Count() == 0 &&
				this.Tags.Except(other.Tags).Count() == 0;
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

			for (int index = 0; index < this.Posts.Count; index++)
				hash.Add(this.Posts[index]);

			for (int index = 0; index < this.Authors.Count; index++)
				hash.Add(this.Authors[index]);

			for (int index = 0; index < this.Tags.Count; index++)
				hash.Add(this.Tags[index]);

			return hash.ToHashCode();
		}
	}
}

