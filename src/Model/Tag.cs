using System;
using System.Collections.Generic;

namespace MSiccDev.ServerlessBlog.Model
{
	public class Tag
	{
		public Guid TagId { get; set; }

		public string Name { get; set; }

		public string Slug { get; set; }

		public List<Blog> Blogs { get; set; }

		public List<Post> Posts { get; set; }

		public List<PostTagMapping> PostTagMappings { get; set; }
	}
}