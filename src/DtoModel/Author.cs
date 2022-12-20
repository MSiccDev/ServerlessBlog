using System;
using Newtonsoft.Json;
namespace MSiccDev.ServerlessBlog.DtoModel
{
    public class Author
    {
        [JsonProperty(Required = Required.Always)]
        public Guid AuthorId { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string DisplayName { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string UserName { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public Medium UserImage { get; set; }
    }
}

