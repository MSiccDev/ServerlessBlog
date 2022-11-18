using System;
using System.Collections.Generic;

namespace MSiccDev.ServerlessBlog.EntityModel
{
    public class MediumType
    {
        public Guid MediumTypeId { get; set; }

        public string MimeType { get; set; }

        public string Name { get; set; }

        public string Encoding { get; set; }

        public ICollection<Medium> Media { get; set; }
    }
}

