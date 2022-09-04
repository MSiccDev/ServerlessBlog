using System;
using System.Collections.Generic;

namespace MSiccDev.ServerlessBlog.Model
{
	public class Author
	{
		public Guid AuthorId { get; set; }

		public string DisplayName { get; set; }

		public string UserName { get; set; }

		public Media UserImage { get; set; }

		public List<Post> Posts { get; set; }

		public List<Blog> Blogs { get; set; }
	}
}