using System;
namespace MSiccDev.ServerlessBlog.Model
{
	public class PostTagMapping
	{
		public Guid PostId { get; set; }
		public Post Post { get; set; }

		public Guid TagId { get; set; }
		public Tag Tag { get; set; }
	}
}

