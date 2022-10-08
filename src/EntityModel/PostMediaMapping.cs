using System;

namespace MSiccDev.ServerlessBlog.EntityModel
{
    public class PostMediumMapping
    {
        public Post Post { get; set; }
        public Guid PostId { get; set; }

        public Medium Medium { get; set; }
        public Guid MediumId { get; set; }

        public bool AsFeatuerdOnPost { get; set; }
    }
}