using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using MSiccDev.ServerlessBlog.EFCore;
using Newtonsoft.Json;

namespace MSiccDev.ServerlessBlog.BlogFunctions
{
    public abstract class BlogFunctionBase
    {
        internal readonly BlogContext _blogContext;

        internal readonly JsonSerializerSettings _jsonSerializerSettings;

        public BlogFunctionBase(BlogContext blogContext)
        {
            _blogContext = blogContext ?? throw new ArgumentNullException(nameof(blogContext));

            _jsonSerializerSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };
        }


        public virtual Task<IActionResult> Create([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log, string blogId)
        {
            throw new NotImplementedException();
        }

        public virtual Task<IActionResult> Get([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, ILogger log, string blogId, string id = null)
        {
            throw new NotImplementedException();
        }

        public virtual Task<IActionResult> Update([HttpTrigger(AuthorizationLevel.Function, "put", Route = null)] HttpRequest req, ILogger log, string blogId, string id)
        {
            throw new NotImplementedException();
        }

        public virtual Task<IActionResult> Delete([HttpTrigger(AuthorizationLevel.Function, "delete", Route = null)] HttpRequest req, ILogger log, string blogId, string id)
        {
            throw new NotImplementedException();
        }

    }
}

