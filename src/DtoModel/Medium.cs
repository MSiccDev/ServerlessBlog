using System;
namespace MSiccDev.ServerlessBlog.DtoModel
{
    public class Medium
    {
        public Guid MediumId { get; set; }

        public Uri MediumUrl { get; set; }

        public string AlternativeText { get; set; }

        public string Description { get; set; }

        public MediumType MediumType { get; set; }

        public bool? IsPostImage { get; set; } = null;
    }
}