using System;
using Newtonsoft.Json;
namespace MSiccDev.ServerlessBlog.DtoModel
{
    public class FileUploadResponse
    {
        [JsonConstructor]
        public FileUploadResponse()
        {

        }

        public FileUploadResponse(Uri url) =>
            this.Url = url;

        public Uri Url { get; set; }
    }
}
