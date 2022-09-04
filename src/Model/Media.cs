using System;
using System.Collections.Generic;

namespace MSiccDev.ServerlessBlog.Model
{
	public class Media
	{
		public Media()
		{
		}

		public Guid MediaId { get; set; }

		public Uri MediaUrl { get; set; }

		public MediaType MediaType { get; set; }

		public Guid BlogId { get; set; }
		public Blog Blog { get; set; }

		public List<Post> Posts { get; set; }

		public List<PostMediaMapping> PostMediaMappings { get; set; }
	}


}

