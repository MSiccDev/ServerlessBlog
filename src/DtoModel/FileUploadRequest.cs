using System;
using Newtonsoft.Json;
namespace MSiccDev.ServerlessBlog.DtoModel
{
    public class FileUploadRequest
    {
        [JsonConstructor]
        public FileUploadRequest()
        {

        }

        public FileUploadRequest(byte[] fileBytes, string fileName, string containerName)
        {
            this.Base64Content = Convert.ToBase64String(fileBytes, Base64FormattingOptions.None);
            this.FileName = fileName;
            this.ContainerName = containerName;
        }

        [JsonProperty(Required = Required.Always)]
        public string Base64Content { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string FileName { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string ContainerName { get; set; }

    }
}
