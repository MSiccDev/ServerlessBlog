using System;
using System.Collections.Generic;

namespace MSiccDev.ServerlessBlog.EntityModel
{
    public class Medium
    {
        public Guid MediumId { get; set; }

        public Uri MediumUrl { get; set; }

        public string AlternativeText { get; set; }

        public string Description { get; set; }

        public Guid MediumTypeId { get; set; }
        public MediumType MediumType { get; set; }

        public Guid BlogId { get; set; }
        public Blog Blog { get; set; }

        public ICollection<Post> Posts { get; set; }
        public ICollection<Author> Authors { get; set; }

        public List<PostMediumMapping> PostMediumMappings { get; set; }

    }
}

