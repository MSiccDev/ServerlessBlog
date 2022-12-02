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
using Microsoft.EntityFrameworkCore;
using MSiccDev.ServerlessBlog.ModelHelper;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace MSiccDev.ServerlessBlog.BlogFunctions
{
    public class TagFunction : BlogFunctionBase
    {
        private const string Route = "blog/{blogId}/tags";

        public TagFunction(BlogContext blogContext) : base(blogContext)
        {
        }

        [FunctionName($"{nameof(TagFunction)}_{nameof(Create)}")]
        public override async Task<IActionResult> Create([HttpTrigger(AuthorizationLevel.Function, new[] { "post" }, Route = Route)] HttpRequest req, ILogger log, string blogId)
        {
            try
            {

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            DtoModel.Tag tag = JsonConvert.DeserializeObject<DtoModel.Tag>(requestBody);

                if (tag != null)
                {
                    EntityModel.Tag newTagEntity = tag.CreateFrom(Guid.Parse(blogId));

                    EntityEntry<EntityModel.Tag> createdTag =
                        _blogContext.Tags.Add(newTagEntity);

                    await _blogContext.SaveChangesAsync();

                    return new CreatedResult($"{req.GetEncodedUrl()}/{createdTag.Entity.TagId}", "OK");
                }
                else
                {
                    return new BadRequestObjectResult("Submitted data is invalid, author cannot be created.");
                }
            }
            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return new BadRequestObjectResult(ex);
            }
        }

        [FunctionName($"{nameof(TagFunction)}_{nameof(Delete)}")]
        public override async Task<IActionResult> Delete([HttpTrigger(AuthorizationLevel.Function, new[] { "delete" }, Route = Route + "/{id}")] HttpRequest req, ILogger log, string blogId, string id)
        {
            try
            {
                EntityModel.Tag existingTag =
                    await _blogContext.Tags.
                            SingleOrDefaultAsync(tag => tag.BlogId == Guid.Parse(blogId) &&
                                                        tag.TagId == Guid.Parse(id));

                if (existingTag == null)
                {
                    log.LogWarning($"Tag with Id {id} not found");
                    return new NotFoundResult();
                }

                _blogContext.Tags.Remove(existingTag);

                await _blogContext.SaveChangesAsync();

                return new OkResult();
            }
            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return new BadRequestObjectResult(ex);
            }
        }

        [FunctionName($"{nameof(TagFunction)}_{nameof(Get)}")]
        public override async Task<IActionResult> Get([HttpTrigger(AuthorizationLevel.Function, new[] { "get" }, Route = Route + "/{id?}")] HttpRequest req, ILogger log, string blogId, string id = null)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(id))
                {
                    EntityModel.Tag existingTag =
                        await _blogContext.Tags.
                                   SingleOrDefaultAsync(tag => tag.BlogId == Guid.Parse(blogId) &&
                                                               tag.TagId == Guid.Parse(id));

                    if (existingTag == null)
                    {
                        log.LogWarning($"Tag with Id {id} not found");
                        return new NotFoundResult();
                    }

                    return new OkObjectResult(JsonConvert.SerializeObject(existingTag.ToDto(), _jsonSerializerSettings));
                }
                else
                {
                    log.LogInformation("Trying to get tags...");

                    var queryParams = req.GetQueryParameterDictionary();

                    if (blogId == default)
                        return new BadRequestObjectResult("Required parameter 'blogid' (GUID) is not specified or cannot be parsed.");

                    List<EntityModel.Tag> entityResultSet = new List<EntityModel.Tag>();

                    (int count, int skip) = req.GetPagingProperties();

                    entityResultSet = await _blogContext.Tags.
                                            Where(author => author.BlogId == Guid.Parse(blogId)).
                                            Skip(skip).
                                            Take(count).
                                            ToListAsync();

                    List<DtoModel.Tag> resultSet = entityResultSet.Select(entity => entity.ToDto()).ToList();

                    return new OkObjectResult(JsonConvert.SerializeObject(resultSet, _jsonSerializerSettings));
                }
            }
            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return new BadRequestObjectResult(ex);
            }
        }

        [FunctionName($"{nameof(TagFunction)}_{nameof(Update)}")]
        public override async Task<IActionResult> Update([HttpTrigger(AuthorizationLevel.Function, new[] { "put" }, Route = Route + "/{id}")] HttpRequest req, ILogger log, string blogId, string id)
        {
            try
            {

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            DtoModel.Tag tag = JsonConvert.DeserializeObject<DtoModel.Tag>(requestBody);

                if (tag != null)
                {
                    EntityModel.Tag existingTag =
                        await _blogContext.Tags.
                                SingleOrDefaultAsync(tag => tag.BlogId == Guid.Parse(blogId) &&
                                                            tag.TagId == Guid.Parse(id));
                    if (existingTag == null)
                    {
                        log.LogWarning($"Tag with Id {id} not found");
                        return new NotFoundResult();
                    }

                    existingTag.UpdateWith(tag);

                    await _blogContext.SaveChangesAsync();

                    return new AcceptedResult();
                }
                else
                {
                    return new BadRequestObjectResult("Submitted data is invalid, tag cannot be modified.");
                }
            }
            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return new BadRequestObjectResult(ex);
            }
        }


    }
}

