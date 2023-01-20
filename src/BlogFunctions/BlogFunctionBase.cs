using Azure.Core.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MSiccDev.ServerlessBlog.EFCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
namespace MSiccDev.ServerlessBlog.BlogFunctions
{
    public abstract class BlogFunctionBase
    {
        internal readonly BlogContext BlogContext;
        internal ILogger Logger;
        internal JsonSerializerSettings? JsonSerializerSettings;

#pragma warning disable CS8618
        protected BlogFunctionBase(BlogContext blogContext)
#pragma warning restore CS8618
        {
            BlogContext = blogContext ?? throw new ArgumentNullException(nameof(blogContext));

            CreateNewtonSoftSerializerSettings();
        }

        private void CreateNewtonSoftSerializerSettings()
        {
            JsonSerializerSettings = NewtonsoftJsonObjectSerializer.CreateJsonSerializerSettings();

            JsonSerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            JsonSerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            JsonSerializerSettings.Formatting = Formatting.Indented;
            JsonSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            JsonSerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            JsonSerializerSettings.DateParseHandling = DateParseHandling.DateTimeOffset;
        }

        public virtual Task<HttpResponseData> Create([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req,
            string blogId)
        {
            throw new NotImplementedException();
        }

        public virtual Task<HttpResponseData> GetList([HttpTriggerAttribute(AuthorizationLevel.Function, "get", Route = null)] HttpRequestData req, string blogId)
        {
            throw new NotImplementedException();
        }

        public virtual Task<HttpResponseData> GetSingle([HttpTriggerAttribute(AuthorizationLevel.Function, "get", Route = null)] HttpRequestData req, string blogId, string id)
        {
            throw new NotImplementedException();
        }

        public virtual Task<HttpResponseData> Update([HttpTriggerAttribute(AuthorizationLevel.Function, "put", Route = null)] HttpRequestData req, string blogId, string id)
        {
            throw new NotImplementedException();
        }

        public virtual Task<HttpResponseData> Delete([HttpTriggerAttribute(AuthorizationLevel.Function, "delete", Route = null)] HttpRequestData req, string blogId, string id)
        {
            throw new NotImplementedException();
        }

    }
}

