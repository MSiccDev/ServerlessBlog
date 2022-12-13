using Microsoft.Extensions.Logging;
using MSiccDev.ServerlessBlog.EFCore;
using Newtonsoft.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;


namespace MSiccDev.ServerlessBlog.BlogFunctions
{
    public abstract class BlogFunctionBase
    {
        internal readonly BlogContext _blogContext;
        internal ILogger _logger;

#pragma warning disable CS8618
        public BlogFunctionBase(BlogContext blogContext)
#pragma warning restore CS8618
        {
            _blogContext = blogContext ?? throw new ArgumentNullException(nameof(blogContext));
        }


        public virtual Task<HttpResponseData> Create(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req,
            string blogId)
        {
            throw new NotImplementedException();
        }

        public virtual Task<HttpResponseData> Get(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequestData req, 
            string blogId, string? id = null)
        {
            throw new NotImplementedException();
        }

        public virtual Task<HttpResponseData> Update(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = null)] HttpRequestData req, 
            string blogId, string id)
        {
            throw new NotImplementedException();
        }

        public virtual Task<HttpResponseData> Delete(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = null)] HttpRequestData req, 
            string blogId, string id)
        {
            throw new NotImplementedException();
        }

    }
}

