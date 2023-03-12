using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using MSiccDev.ServerlessBlog.DtoModel;
using MSiccDev.ServerlessBlog.EFCore;
using MSiccDev.ServerlessBlog.ModelHelper;
using Newtonsoft.Json;
namespace MSiccDev.ServerlessBlog.BlogFunctions
{
    public class TagFunction : BlogFunctionBase
    {
        private const string Route = "blog/{blogId}/tag";

        public TagFunction(BlogContext blogContext, ILoggerFactory loggerFactory) : base(blogContext) =>
            Logger = loggerFactory.CreateLogger<TagFunction>();

        [OpenApiOperation("CREATE", "Tag", Description = "Creates a new tag for the specified blog in the database.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("blogId", Type = typeof(Guid?), Required = true, Description = "Id of the blog the new tag should live in")]
        [OpenApiRequestBody("application/json", typeof(Tag), Required = true, Description = "Tag object to be created")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Created, Description = "Created Response if succeeded")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Response for unauthenticated requests.")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "Request cannot not be processed, see response body why")]
        [Function($"{nameof(TagFunction)}_{nameof(Create)}")]
        public override async Task<HttpResponseData> Create([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Route)] HttpRequestData req, string blogId)
        {
            try
            {

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                Tag? tag = JsonConvert.DeserializeObject<Tag>(requestBody);

                if (tag != null)
                {
                    EntityModel.Tag newTagEntity = tag.CreateFrom(Guid.Parse(blogId));

                    EntityEntry<EntityModel.Tag> createdTag =
                        BlogContext.Tags.Add(newTagEntity);

                    await BlogContext.SaveChangesAsync();

                    return await req.CreateNewEntityCreatedResponseDataAsync(createdTag.Entity.TagId);
                }
                else
                {
                    return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Submitted data is invalid, tag cannot be created.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating tag object on blog with Id {BlogId}", blogId);
                return await req.CreateResponseDataAsync(HttpStatusCode.InternalServerError, "An internal server error occured. Error details logged.");
            }
        }


        [OpenApiOperation("GET", "Tag", Description = "Gets a list of tags from the database.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("blogId", Type = typeof(Guid?), Required = true, Description = "Id of the blog on which the tags exist", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("skip", Type = typeof(int), Required = true, Description = "skips the specified amount of entries from the results", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("count", Type = typeof(int), Required = true, Description = "how many results are being returned per request", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(Tag), Description = "Gets a list of tags")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Response for unauthenticated requests.")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "Request cannot not be processed, see response body why")]
        [Function($"{nameof(TagFunction)}_{nameof(GetList)}")]
        public override async Task<HttpResponseData> GetList([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route)] HttpRequestData req, string blogId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(blogId) || Guid.Parse(blogId) == default)
                    return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Required parameter 'blogId' (GUID) is not specified or cannot be parsed.");

                Logger.LogInformation("Trying to get tags...");

                (int count, int skip) = req.GetPagingProperties();

                List<EntityModel.Tag> entityResultSet = await BlogContext.Tags.
                                                                          Where(tag => tag.BlogId == Guid.Parse(blogId)).
                                                                          Skip(skip).
                                                                          Take(count).
                                                                          ToListAsync();

                List<Tag> resultSet = entityResultSet.Select(entity => entity.ToDto()).ToList();

                return await req.CreateOkResponseDataWithJsonAsync(resultSet, JsonSerializerSettings);
                
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting tag list for blog with Id \'{Id}\'", blogId);
                return await req.CreateResponseDataAsync(HttpStatusCode.InternalServerError, "An internal server error occured. Error details logged.");
            }
        }


        [OpenApiOperation("GET", "Tag", Description = "Gets one or more tags from the database.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("blogId", Type = typeof(Guid?), Required = true, Description = "Id of the blog on which the tag(s) exists", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("id", Type = typeof(Guid?), Required = false, Description = "Id of the desired tag, otherwise none", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(Tag), Description = "Gets a list of tags when no Id is specified or a single tag filtered by the specified Id")]
        [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "No tag with the specified id was found on this blog")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Response for unauthenticated requests.")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "Request cannot not be processed, see response body why")]
        [Function($"{nameof(TagFunction)}_{nameof(GetSingle)}")]
        public override async Task<HttpResponseData> GetSingle([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route + "/{id}")] HttpRequestData req, string blogId, string id)
        {
            try
            {
                if ((string.IsNullOrWhiteSpace(blogId) || Guid.Parse(blogId) == default))
                    return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest,"Required parameter 'blogId' (GUID) is not specified or cannot be parsed.");
                
                if (!string.IsNullOrWhiteSpace(id))
                {
                    Logger.LogInformation("Trying to get tag with Id {Id}...", id);

                    EntityModel.Tag? existingTag =
                        await BlogContext.Tags.
                                          SingleOrDefaultAsync(tag => tag.BlogId == Guid.Parse(blogId) &&
                                                                      tag.TagId == Guid.Parse(id));

                    if (existingTag == null)
                    {
                        Logger.LogWarning("Tag with Id {Id} not found", id);
                        return req.CreateResponse(HttpStatusCode.NotFound);
                    }

                    return await req.CreateOkResponseDataWithJsonAsync(existingTag.ToDto(), JsonSerializerSettings);
                }

                return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Submitted data is invalid, must specify BlogId");

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting tag with Id '{TagId}' for blog with Id \'{BlogId}\'", id, blogId);
                return await req.CreateResponseDataAsync(HttpStatusCode.InternalServerError, "An internal server error occured. Error details logged.");
            }
        }


        [OpenApiOperation("UPDATE", "Tag", Description = "Updates an existing tag of the specified blog in the database.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("blogId", Type = typeof(Guid?), Required = true, Description = "Id of the blog on which the tag exists", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(Tag), Required = true, Description = "Tag object to be updated")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Accepted, Description = "Accepted if the update operation succeeded")]
        [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "No tag with the specified id was found on this blog")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Response for unauthenticated requests.")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "Request cannot not be processed, see response body why")]
        [Function($"{nameof(TagFunction)}_{nameof(Update)}")]
        public override async Task<HttpResponseData> Update([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = Route + "/{id}")] HttpRequestData req, string blogId, string id)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                Tag? tagToUpdate = JsonConvert.DeserializeObject<Tag>(requestBody);

                if (tagToUpdate != null)
                {
                    EntityModel.Tag? existingTag =
                        await BlogContext.Tags.
                                          SingleOrDefaultAsync(tag => tag.BlogId == Guid.Parse(blogId) &&
                                                                      tag.TagId == Guid.Parse(id));
                    if (existingTag == null)
                    {
                        Logger.LogWarning("Tag with Id {Id} not found", id);
                        return req.CreateResponse(HttpStatusCode.NotFound);
                    }

                    existingTag.UpdateWith(tagToUpdate);

                    await BlogContext.SaveChangesAsync();

                    return req.CreateResponse(HttpStatusCode.Accepted);
                }
                else
                {
                    return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest,"Submitted data is invalid, tag cannot be modified.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating tag with Id '{TagId}' for blog with Id \'{BlogId}\'", id, blogId);
                return await req.CreateResponseDataAsync(HttpStatusCode.InternalServerError, "An internal server error occured. Error details logged.");
            }
        }


        [OpenApiOperation("DELETE", "Tag", Description = "Deletes an existing tag from the specified blog in the database.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("blogId", Type = typeof(Guid?), Required = true, Description = "Id of the blog on which the tag exists", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "OK Response if succeeded")]
        [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "No tag with the specified id was found on this blog")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Response for unauthenticated requests.")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "Request cannot not be processed, see response body why")]
        [Function($"{nameof(TagFunction)}_{nameof(Delete)}")]
        public override async Task<HttpResponseData> Delete([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = Route + "/{id}")] HttpRequestData req, string blogId, string id)
        {
            try
            {
                EntityModel.Tag? existingTag =
                    await BlogContext.Tags.
                                      SingleOrDefaultAsync(tag => tag.BlogId == Guid.Parse(blogId) &&
                                                                  tag.TagId == Guid.Parse(id));

                if (existingTag == null)
                {
                    Logger.LogWarning("Tag with Id {Id} not found", id);
                    return req.CreateResponse(HttpStatusCode.NotFound);
                }

                BlogContext.Tags.Remove(existingTag);

                await BlogContext.SaveChangesAsync();

                return req.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating tag with Id '{TagId}' from blog with Id \'{BlogId}\'", id, blogId);
                return await req.CreateResponseDataAsync(HttpStatusCode.InternalServerError, "An internal server error occured. Error details logged.");
            }
        }

    }
}

