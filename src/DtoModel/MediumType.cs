using System;

namespace DtoModel
{
    public class MediumType
    {
        public Guid MediumTypeId { get; set; }

        public string MimeType { get; set; }

        public string Name { get; set; }

        public string Encoding { get; set; }
    }
}