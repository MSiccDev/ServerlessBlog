using System;
using System.Collections.Generic;

namespace MSiccDev.ServerlessBlog.EntityModel
{
    public class Tag
    {
        public Guid TagId { get; set; }

        public string Name { get; set; }

        public string Slug { get; set; }

        public Guid BlogId { get; set; }
        public Blog Blog { get; set; }

        public ICollection<Post> Posts { get; set; }

        public List<PostTagMapping> PostTagMappings { get; set; }

    }
}