﻿using System;
namespace MSiccDev.ServerlessBlog.DtoModel
{
    public class Author
    {
        public Guid AuthorId { get; set; }

        public string DisplayName { get; set; }

        public string UserName { get; set; }

        public Medium UserImage { get; set; }
    }
}

