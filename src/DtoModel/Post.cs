using System;
using System.Collections.Generic;

namespace DtoModel
{
    public class Post
    {
        public Guid PostId { get; set; }

        public string Title { get; set; }
        public string Content { get; set; }

        public string Slug { get; set; }

        public DateTimeOffset Published { get; set; }
        public DateTimeOffset LastModified { get; set; }

        public Medium PostImage { get; set; }

        public Author Author { get; set; }

        public Guid BlogId { get; set; }

        public List<Tag> Tags { get; set; }

        public List<Medium> Media { get; set; }
    }
}