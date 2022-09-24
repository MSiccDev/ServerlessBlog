using System;

namespace DtoModel
{
    public class Media
    {
        public Guid MediaId { get; set; }

        public Uri MediaUrl { get; set; }

        public string AlternativeText { get; set; }

        public string Description { get; set; }

        public MediaType MediaType { get; set; }
    }
}