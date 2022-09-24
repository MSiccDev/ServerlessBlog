using System;

namespace DtoModel
{
    public class MediaType
    {
        public Guid MediaTypeId { get; set; }

        public string MimeType { get; set; }

        public string Name { get; set; }

        public string Encoding { get; set; }
    }
}