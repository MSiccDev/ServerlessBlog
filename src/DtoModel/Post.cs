using System;
using System.Collections.Generic;
using Newtonsoft.Json;
namespace MSiccDev.ServerlessBlog.DtoModel
{
	public class Post : DtoModelBase
    {
	    [JsonProperty(Required = Required.Always)]
	    public override Guid? BlogId { get => base.BlogId; set => base.BlogId = value; }

	    [JsonProperty(Required = Required.Always)]
	    public override Guid? ResourceId { get => base.ResourceId; set => base.ResourceId = value; }

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

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Tag> Tags { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Medium> Media { get; set; }
    }
}