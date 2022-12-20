using System;
using System.Collections.Generic;
using Newtonsoft.Json;
namespace MSiccDev.ServerlessBlog.DtoModel
{
    public class Post
    {
        [JsonProperty(Required = Required.Always)]
        public Guid PostId { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Title { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Content { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Slug { get; set; }

        [JsonProperty(Required = Required.DisallowNull)]
        public DateTimeOffset Published { get; set; }

        [JsonProperty(Required = Required.Always)]
        public DateTimeOffset LastModified { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public Medium PostImage { get; set; }

        [JsonProperty(Required = Required.Always)]
        public Author Author { get; set; }

        [JsonProperty(Required = Required.Always)]
        public Guid BlogId { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public List<Tag> Tags { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public List<Medium> Media { get; set; }
    }
}