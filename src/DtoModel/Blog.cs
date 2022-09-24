using System;
using System.Collections.Generic;

namespace DtoModel
{
    public class Blog
    {
        public Guid BlogId { get; set; }

        public string Name { get; set; }

        public string Slogan { get; set; }

        public Uri LogoUrl { get; set; }

        public List<Post> Posts { get; set; }

        public List<Author> Authors { get; set; }

        public List<Tag> Tags { get; set; }

        public List<Media> Media { get; set; }
    }
}