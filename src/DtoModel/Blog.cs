using System;
using System.Collections.Generic;
using Newtonsoft.Json;
namespace MSiccDev.ServerlessBlog.DtoModel
{
    public class Blog : DtoModelBase
    {
        [JsonProperty(Required = Required.Always)]
        public override Guid? BlogId { get => base.BlogId; set => base.BlogId = value; }

        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Slogan { get; set; }

        [JsonProperty(Required = Required.Always)]
        public Uri LogoUrl { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Post> Posts { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Author> Authors { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Tag> Tags { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Medium> Media { get; set; }
    }
}