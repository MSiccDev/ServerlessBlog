using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;

namespace MSiccDev.ServerlessBlog.EntityModel
{
    public class Post
    {
        public Guid PostId { get; set; }

        public string Title { get; set; }
        public string Content { get; set; }

        public string Slug { get; set; }

        public DateTimeOffset Published { get; set; }
        public DateTimeOffset LastModified { get; set; }

        public Author Author { get; set; }
        public Guid AuthorId { get; set; }

        public Blog Blog { get; set; }
        public Guid BlogId { get; set; }

        public ICollection<Tag> Tags { get; set; }
        public ICollection<Medium> Media { get; set; }

        public List<PostTagMapping> PostTagMappings { get; set; }
        public List<PostMediumMapping> PostMediumMappings { get; set; }

    }
}