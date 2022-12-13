using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MSiccDev.ServerlessBlog.EFCore;
using Microsoft.EntityFrameworkCore;
using MSiccDev.ServerlessBlog.ModelHelper;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace MSiccDev.ServerlessBlog.BlogFunctions
{
    public class TagFunction : BlogFunctionBase
    {
        private const string Route = "blog/{blogId}/tags";

        public TagFunction(BlogContext blogContext, ILoggerFactory loggerFactory) : base(blogContext)
        {
            _logger = loggerFactory.CreateLogger<TagFunction>();
        }

        [Function($"{nameof(TagFunction)}_{nameof(Create)}")]
        public override async Task<HttpResponseData> Create([HttpTrigger(AuthorizationLevel.Function, new[] { "post" }, Route = Route)] HttpRequestData req, string blogId)
        {
            try
            {

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                DtoModel.Tag? tag = JsonConvert.DeserializeObject<DtoModel.Tag>(requestBody);

                if (tag != null)
                {
                    EntityModel.Tag newTagEntity = tag.CreateFrom(Guid.Parse(blogId));

                    EntityEntry<EntityModel.Tag> createdTag =
                        _blogContext.Tags.Add(newTagEntity);

                    await _blogContext.SaveChangesAsync();

                    return await req.CreateNewEntityCreatedResponseDataAsync(createdTag.Entity.TagId);
                }
                else
                {
                    return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest,"Submitted data is invalid, author cannot be created.");
                }
            }
            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, ex.ToString());
            }
        }

        [Function($"{nameof(TagFunction)}_{nameof(Delete)}")]
        public override async Task<HttpResponseData> Delete([HttpTrigger(AuthorizationLevel.Function, new[] { "delete" }, Route = Route + "/{id}")] HttpRequestData req, string blogId, string id)
        {
            try
            {
                EntityModel.Tag? existingTag =
                    await _blogContext.Tags.
                            SingleOrDefaultAsync(tag => tag.BlogId == Guid.Parse(blogId) &&
                                                        tag.TagId == Guid.Parse(id));

                if (existingTag == null)
                {
                    _logger.LogWarning($"Tag with Id {id} not found");
                    return req.CreateResponse(HttpStatusCode.NotFound);
                }

                _blogContext.Tags.Remove(existingTag);

                await _blogContext.SaveChangesAsync();

                return req.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, ex.ToString());
            }
        }

        [Function($"{nameof(TagFunction)}_{nameof(Get)}")]
        public override async Task<HttpResponseData> Get([HttpTrigger(AuthorizationLevel.Function, new[] { "get" }, Route = Route + "/{id?}")] HttpRequestData req, string blogId, string? id = null)
        {
            try
            {
                if ((string.IsNullOrWhiteSpace(blogId) || Guid.Parse(blogId) == default))
                    return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest,"Required parameter 'blogId' (GUID) is not specified or cannot be parsed.");
                
                if (!string.IsNullOrWhiteSpace(id))
                {
                    EntityModel.Tag? existingTag =
                        await _blogContext.Tags.
                                   SingleOrDefaultAsync(tag => tag.BlogId == Guid.Parse(blogId) &&
                                                               tag.TagId == Guid.Parse(id));

                    if (existingTag == null)
                    {
                        _logger.LogWarning($"Tag with Id {id} not found");
                        return req.CreateResponse(HttpStatusCode.NotFound);
                    }

                    return await req.CreateOKResponseDataWithJsonAsync(existingTag.ToDto());
                }
                else
                {
                    _logger.LogInformation("Trying to get tags...");

                    var queryParams = req.GetQueryParameterDictionary();

                    
                    (int count, int skip) = req.GetPagingProperties();

                    List<EntityModel.Tag> entityResultSet = await _blogContext.Tags.
                                                                              Where(author => author.BlogId == Guid.Parse(blogId)).
                                                                              Skip(skip).
                                                                              Take(count).
                                                                              ToListAsync();

                    List<DtoModel.Tag> resultSet = entityResultSet.Select(entity => entity.ToDto()).ToList();

                    return await req.CreateOKResponseDataWithJsonAsync(resultSet);
                }
            }
            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, ex.ToString());
            }
        }

        [Function($"{nameof(TagFunction)}_{nameof(Update)}")]
        public override async Task<HttpResponseData> Update([HttpTrigger(AuthorizationLevel.Function, new[] { "put" }, Route = Route + "/{id}")] HttpRequestData req, string blogId, string id)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                DtoModel.Tag? tag = JsonConvert.DeserializeObject<DtoModel.Tag>(requestBody);

                if (tag != null)
                {
                    EntityModel.Tag? existingTag =
                        await _blogContext.Tags.
                                SingleOrDefaultAsync(tag => tag.BlogId == Guid.Parse(blogId) &&
                                                            tag.TagId == Guid.Parse(id));
                    if (existingTag == null)
                    {
                        _logger.LogWarning($"Tag with Id {id} not found");
                        return req.CreateResponse(HttpStatusCode.NotFound);
                    }

                    existingTag.UpdateWith(tag);

                    await _blogContext.SaveChangesAsync();

                    return req.CreateResponse(HttpStatusCode.Accepted);
                }
                else
                {
                    return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest,"Submitted data is invalid, tag cannot be modified.");
                }
            }
            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest,ex.ToString());
            }
        }


    }
}

