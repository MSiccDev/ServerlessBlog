using System;
namespace MSiccDev.ServerlessBlog.DtoModel
{
    public class Tag
    {
        public Guid TagId { get; set; }

        public string Name { get; set; }

        public string Slug { get; set; }
    }
}