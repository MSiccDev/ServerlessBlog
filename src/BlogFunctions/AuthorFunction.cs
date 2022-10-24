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
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MSiccDev.ServerlessBlog.ModelHelper;
using Microsoft.EntityFrameworkCore;
using MSiccDev.ServerlessBlog.MappingHelper;
using System.Collections.Generic;
using System.Linq;

namespace MSiccDev.ServerlessBlog.BlogFunctions
{
    public class AuthorFunction : BlogFunctionBase
    {
        private const string Route = "blog/{blogId}/author";

        public AuthorFunction(BlogContext blogContext) : base(blogContext)
        {
        }

        [FunctionName($"{nameof(AuthorFunction)}_{nameof(Create)}")]
        public override Task<IActionResult> Create([HttpTrigger(AuthorizationLevel.Function, new[] { "post" }, Route = Route)] HttpRequest req, ILogger log, string blogId)
        {
            return base.Create(req, log, blogId);
        }

        [FunctionName($"{nameof(AuthorFunction)}_{nameof(Delete)}")]
        public override Task<IActionResult> Delete([HttpTrigger(AuthorizationLevel.Function, new[] { "delete" }, Route = Route + "/{id}")] HttpRequest req, ILogger log, string blogId, string id)
        {
            return base.Delete(req, log, blogId, id);
        }

        [FunctionName($"{nameof(AuthorFunction)}_{nameof(Get)}")]
        public override async Task<IActionResult> Get([HttpTrigger(AuthorizationLevel.Function, new[] { "get" }, Route = Route + "/{id?}")] HttpRequest req, ILogger log, string blogId, string id = null)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(id))
                {
                    EntityModel.Author existingAuthor =
                            await _blogContext.Authors.
                                   Include(author => author.UserImage).
                                   ThenInclude(media => media.MediumType).
                                   SingleOrDefaultAsync(author => author.BlogId == Guid.Parse(blogId) &&
                                                                  author.AuthorId == Guid.Parse(id));

                    if (existingAuthor == null)
                    {
                        log.LogWarning($"Author with Id {id} not found");
                        return new NotFoundResult();
                    }

                    return new OkObjectResult(JsonConvert.SerializeObject(existingAuthor.ToDto(), _jsonSerializerSettings));
                }
                else
                {
                    log.LogInformation("Trying to get authors...");

                    var queryParams = req.GetQueryParameterDictionary();

                    if (blogId == default)
                        return new BadRequestObjectResult("Required parameter 'blogid' (GUID) is not specified or cannot be parsed.");

                    List<EntityModel.Author> entityResultSet = new List<EntityModel.Author>();

                    (int count, int skip) = req.GetPagingProperties();

                    entityResultSet = await _blogContext.Authors.
                                            Where(author => author.BlogId == Guid.Parse(blogId)).
                                            Skip(skip).
                                            Take(count).
                                            ToListAsync();

                    List<DtoModel.Author> resultSet = entityResultSet.Select(entity => entity.ToDto()).ToList();

                    return new OkObjectResult(JsonConvert.SerializeObject(resultSet, _jsonSerializerSettings));
                }
            }
            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return new BadRequestObjectResult(ex);
            }
        }



        [FunctionName($"{nameof(AuthorFunction)}_{nameof(Update)}")]
        public override Task<IActionResult> Update([HttpTrigger(AuthorizationLevel.Function, new[] { "put" }, Route = Route + "/{id}")] HttpRequest req, ILogger log, string blogId, string id)
        {
            return base.Update(req, log, blogId, id);
        }
    }
}

