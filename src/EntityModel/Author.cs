using System;
using System.Collections.Generic;
using System.Linq;

namespace MSiccDev.ServerlessBlog.EntityModel
{
    public class Author
    {
        public Guid AuthorId { get; set; }

        public string DisplayName { get; set; }

        public string UserName { get; set; }

        public Medium UserImage { get; set; }
        public Guid? UserImageId { get; set; }

        public Blog Blog { get; set; }
        public Guid BlogId { get; set; }

        public ICollection<Post> Posts { get; set; }


    }
}