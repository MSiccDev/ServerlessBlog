using System;
using Newtonsoft.Json;
namespace MSiccDev.ServerlessBlog.DtoModel
{
    public class Medium : DtoModelBase
    {
		[JsonProperty(Required = Required.Always)]
		public override Guid? BlogId { get => base.BlogId; set => base.BlogId = value; }

		[JsonProperty(Required = Required.Always)]
		public override Guid? ResourceId { get => base.ResourceId; set => base.ResourceId = value; }

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