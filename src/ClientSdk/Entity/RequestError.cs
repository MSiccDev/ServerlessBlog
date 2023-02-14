using System.Net;
using Newtonsoft.Json;
namespace MSiccDev.ServerlessBlog.ClientSdk
{
    public class RequestError
    {
        [JsonConstructor]
        public RequestError()
        {

        }

        public RequestError(HttpStatusCode? statusCode, string? message)
        {
            this.StatusCode = statusCode;
            this.Message = message;
        }

        [JsonProperty("statusCode")]
        public HttpStatusCode? StatusCode { get; set; }

        [JsonProperty("message")]
        public string? Message { get; set; }


        public override string ToString() =>
            // ReSharper disable HeapView.BoxingAllocation
            $"StatusCode {this.StatusCode.GetValueOrDefault()}: {this.Message}";
        // ReSharper restore HeapView.BoxingAllocation
    }
}
