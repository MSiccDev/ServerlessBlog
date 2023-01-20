﻿using System;
using Newtonsoft.Json;
namespace MSiccDev.ServerlessBlog.DtoModel
{
    public class Tag : DtoModelBase
    {
		[JsonProperty(Required = Required.Always)]
		public override Guid? BlogId { get => base.BlogId; set => base.BlogId = value; }

		[JsonProperty(Required = Required.Always)]
		public override Guid? ResourceId { get => base.ResourceId; set => base.ResourceId = value; }

		[JsonProperty(Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Slug { get; set; }
    }
}