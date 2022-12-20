using System;
using System.Collections.Generic;
using Newtonsoft.Json;
namespace MSiccDev.ServerlessBlog.DtoModel
{
    public class Blog
    {
        [JsonProperty(Required = Required.Always)]
        public Guid BlogId { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Slogan { get; set; }

        [JsonProperty(Required = Required.Always)]
        public Uri LogoUrl { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public List<Post> Posts { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public List<Author> Authors { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public List<Tag> Tags { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public List<Medium> Media { get; set; }
    }
}