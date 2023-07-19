using Newtonsoft.Json;
namespace MSiccDev.ServerlessBlog.DtoModel
{
    public class FileUploadResponse
    {
        [JsonConstructor]
        public FileUploadResponse()
        {

        }

        public FileUploadResponse(string fileName, string containerName)
        {
            this.FileName = fileName;
            this.ContainerName = containerName;
        }

        [JsonProperty(Required = Required.Always)]
        public string FileName { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string ContainerName { get; set; }
    }
}
