using System;
using Newtonsoft.Json;
namespace MSiccDev.ServerlessBlog.DtoModel
{
    public class Tag
    {
        [JsonProperty(Required = Required.Always)]
        public Guid TagId { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Slug { get; set; }
    }
}