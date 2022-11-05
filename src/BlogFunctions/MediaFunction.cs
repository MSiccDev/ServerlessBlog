using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MSiccDev.ServerlessBlog.EFCore;

namespace MSiccDev.ServerlessBlog.BlogFunctions
{
    public class MediaFunction : BlogFunctionBase
    {
        private const string Route = "blog/{blogId}/media";

        public MediaFunction(BlogContext blogContext) : base(blogContext)
        {
        }

        [FunctionName($"{nameof(MediaFunction)}_{nameof(Create)}")]
        public override Task<IActionResult> Create([HttpTrigger(AuthorizationLevel.Function, new[] { "post" }, Route = Route)] HttpRequest req, ILogger log, string blogId)
        {
            return base.Create(req, log, blogId);
        }

        [FunctionName($"{nameof(MediaFunction)}_{nameof(Delete)}")]
        public override Task<IActionResult> Delete([HttpTrigger(AuthorizationLevel.Function, new[] { "delete" }, Route = Route + "/{id}")] HttpRequest req, ILogger log, string blogId, string id)
        {
            return base.Delete(req, log, blogId, id);
        }

        [FunctionName($"{nameof(MediaFunction)}_{nameof(Get)}")]
        public override Task<IActionResult> Get([HttpTrigger(AuthorizationLevel.Function, new[] { "get" }, Route = Route + "/{id?}")] HttpRequest req, ILogger log, string blogId, string id = null)
        {
            return base.Get(req, log, blogId, id);
        }

        [FunctionName($"{nameof(MediaFunction)}_{nameof(Update)}")]
        public override Task<IActionResult> Update([HttpTrigger(AuthorizationLevel.Function, new[] { "put" }, Route = Route + "/{id}")] HttpRequest req, ILogger log, string blogId, string id)
        {
            return base.Update(req, log, blogId, id);
        }
    }
}

