﻿using System;
using Newtonsoft.Json;
namespace MSiccDev.ServerlessBlog.DtoModel
{
    public class MediumType
    {
        [JsonProperty(Required = Required.Always)]
        public Guid MediumTypeId { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string MimeType { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public string Encoding { get; set; }
    }
}