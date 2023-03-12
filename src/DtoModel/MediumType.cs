using System;
using Newtonsoft.Json;
namespace MSiccDev.ServerlessBlog.DtoModel
{
    public class MediumType : DtoModelBase
    {

	    [JsonProperty(Required = Required.Default)]
		public override Guid? BlogId { get => base.BlogId; set => base.BlogId = value; }

		[JsonProperty(Required = Required.Always)]
		public override Guid? ResourceId { get => base.ResourceId; set => base.ResourceId = value; }

		[JsonProperty(Required = Required.Always)]
        public string MimeType { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public string Encoding { get; set; }
    }
}