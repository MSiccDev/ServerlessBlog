using System;
using Newtonsoft.Json;
namespace MSiccDev.ServerlessBlog.DtoModel
{
    public class Medium
    {
        [JsonProperty(Required = Required.Always)]
        public Guid MediumId { get; set; }

        [JsonProperty(Required = Required.Always)]
        public Uri MediumUrl { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public string AlternativeText { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public string Description { get; set; }

        [JsonProperty(Required = Required.Always)]
        public MediumType MediumType { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public bool? IsPostImage { get; set; } = null;
    }
}