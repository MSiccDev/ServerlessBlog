using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MSiccDev.ServerlessBlog.EntityModel
{
    public class Blog
    {
        public Guid BlogId { get; set; }

        public string Name { get; set; }

        public string Slogan { get; set; }

        public Uri LogoUrl { get; set; }

        public ICollection<Post> Posts { get; set; }

        public ICollection<Author> Authors { get; set; }

        public ICollection<Tag> Tags { get; set; }

        public ICollection<Medium> Media { get; set; }

    }
}

