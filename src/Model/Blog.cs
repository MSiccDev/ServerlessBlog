using System;
using System.Collections.Generic;

namespace MSiccDev.ServerlessBlog.Model
{
	public class Blog
	{
		public Blog()
		{
		}

		public Guid BlogId { get; set; }

		public string Name { get; set; }

		public string Slogan { get; set; }

		public Uri LogoUrl { get; set; }

		public List<Post> Posts { get; set; } = new List<Post>();

		public List<Author> Authors { get; set; } = new List<Author>();

		public List<Tag> Tags { get; set; } = new List<Tag>();
	}
}

