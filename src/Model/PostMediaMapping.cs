using System;
namespace MSiccDev.ServerlessBlog.Model
{
	public class PostMediaMapping
	{
		public Guid PostId { get; set; }
		public Post Post { get; set; }

		public Guid MediaId { get; set; }
		public Media Media { get; set; }
	}
}

