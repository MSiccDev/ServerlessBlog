using System;
using System.Collections.Generic;
using System.Net;

namespace MSiccDev.ServerlessBlog.Model
{
	public class Post
	{
		public Guid PostId { get; set; }

		public string Title { get; set; }
		public string Content { get; set; }

		public string Slug { get; set; }

		public DateTimeOffset LastModified { get; set; }

		public Media PostImage { get; set; }
		public Guid PostImageMediaId { get; set; }

		public Author Author { get; set; }
		public Guid AuthorId { get; set; }

		public Guid BlogId { get; set; }
		public Blog Blog { get; set; }


		public List<Tag> Tags { get; set; } = new List<Tag>();

		public List<Media> Media { get; set; } = new List<Media>();

		public List<PostMediaMapping> PostMediaMappings { get; set; }
		public List<PostTagMapping> PostTagMappings { get; set; }
	}
}