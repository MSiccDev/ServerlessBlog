using System;
using System.Collections.Generic;

namespace DtoModel
{
    public class Tag
    {
        public Guid TagId { get; set; }

        public string Name { get; set; }

        public string Slug { get; set; }
    }
}