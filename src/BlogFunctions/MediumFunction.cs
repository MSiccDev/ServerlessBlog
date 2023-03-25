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
    public class MediumFunction : BlogFunctionBase
    {
        private const string Route = "blog/{blogId}/media";

        public MediumFunction(BlogContext blogContext, ILoggerFactory loggerFactory) : base(blogContext) =>
            Logger = loggerFactory.CreateLogger<MediumFunction>();

        [OpenApiOperation("CREATE", "Medium", Description = "Creates a new medium for the specified blog in the database.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("blogId", Type = typeof(Guid), Required = true, Description = "Id of the blog the new medium should live in")]
        [OpenApiRequestBody("application/json", typeof(Medium), Required = true, Description = "Medium object to be created")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Created, Description = "Created Response if succeeded")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Response for unauthenticated requests.")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "Request cannot not be processed, see response body why")]
        [Function($"{nameof(MediumFunction)}_{nameof(Create)}")]
        public override async Task<HttpResponseData> Create([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Route)] HttpRequestData req, string blogId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(blogId) || Guid.Parse(blogId) == default)
                    return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Required parameter 'blogId' (GUID) is not specified or cannot be parsed.");

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                Medium? medium = JsonConvert.DeserializeObject<Medium>(requestBody);

                if (medium != null)
                {
                    EntityModel.Medium newMedium = medium.CreateFrom(Guid.Parse(blogId));

                    EntityEntry<EntityModel.Medium> createdMedium =
                        BlogContext.Media.Add(newMedium);

                    await BlogContext.SaveChangesAsync();

                    return await req.CreateNewEntityCreatedResponseDataAsync(createdMedium.Entity.MediumId);
                }
                return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Submitted data is invalid, media cannot be created.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating medium object on blog with Id {BlogId}", blogId);
                return await req.CreateResponseDataAsync(HttpStatusCode.InternalServerError, "An internal server error occured. Error details logged.");
            }
        }


        [OpenApiOperation("GET", "Medium", Description = "Gets media from the database.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("blogId", Type = typeof(Guid), Required = true, Description = "Id of the blog on which the media exist", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("skip", Type = typeof(int), Required = true, Description = "skips the specified amount of entries from the results", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("count", Type = typeof(int), Required = true, Description = "how many results are being returned per request", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(Medium), Description = "Gets a list of media")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Response for unauthenticated requests.")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "Request cannot not be processed, see response body why")]
        [Function($"{nameof(MediumFunction)}_{nameof(GetList)}")]
        public override async Task<HttpResponseData> GetList([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route)] HttpRequestData req, string blogId)
        {
            try
            {
                Logger.LogInformation("Trying to get media...");

                if (string.IsNullOrWhiteSpace(blogId) || Guid.Parse(blogId) == default)
                    return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Required parameter 'blogId' (GUID) is not specified or cannot be parsed.");

                (int count, int skip) = req.GetPagingProperties();

                List<EntityModel.Medium> entityResultSet = await BlogContext.Media.
                                                                             Include(medium => medium.MediumType).
                                                                             Where(media => media.BlogId == Guid.Parse(blogId)).
                                                                             Skip(skip).
                                                                             Take(count).
                                                                             ToListAsync();

                List<Medium> resultSet = entityResultSet.Select(entity => entity.ToDto()).ToList();

                return await req.CreateOkResponseDataWithJsonAsync(resultSet, JsonSerializerSettings);

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting medium list for blog with Id \'{Id}\'", blogId);
                return await req.CreateResponseDataAsync(HttpStatusCode.InternalServerError, "An internal server error occured. Error details logged.");
            }

        }


        [OpenApiOperation("GET", "Medium", Description = "Gets a medium from the database.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("blogId", Type = typeof(Guid), Required = true, Description = "Id of the blog on which the medium exists", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("id", Type = typeof(Guid), Required = false, Description = "Id of the desired medium", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(Medium), Description = "Gets a a single medium filtered by the specified Id")]
        [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "No medium with the specified id was found on this blog")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Response for unauthenticated requests.")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "Request cannot not be processed, see response body why")]
        [Function($"{nameof(MediumFunction)}_{nameof(GetSingle)}")]
        public override async Task<HttpResponseData> GetSingle([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route + "/{id}")] HttpRequestData req, string blogId, string id)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(id))
                {
                    Logger.LogInformation("Trying to get medium with Id {Id}...", id);

                    if (string.IsNullOrWhiteSpace(blogId) || Guid.Parse(blogId) == default)
                        return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Required parameter 'blogId' (GUID) is not specified or cannot be parsed.");

                    EntityModel.Medium? existingMedium =
                        await BlogContext.Media.
                                          Include(medium => medium.MediumType).
                                          SingleOrDefaultAsync(medium => medium.BlogId == Guid.Parse(blogId) &&
                                                                         medium.MediumId == Guid.Parse(id));

                    if (existingMedium == null)
                    {
                        Logger.LogWarning("Medium with Id {Id} not found", id);
                        return req.CreateResponse(HttpStatusCode.NotFound);
                    }

                    return await req.CreateOkResponseDataWithJsonAsync(existingMedium.ToDto(), JsonSerializerSettings);
                }

                return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Submitted data is invalid, must specify BlogId");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting medium with Id '{MediumId}' for blog with Id \'{BlogId}\'", id, blogId);
                return await req.CreateResponseDataAsync(HttpStatusCode.InternalServerError, "An internal server error occured. Error details logged.");
            }

        }


        [OpenApiOperation("UPDATE", "Medium", Description = "Updates an existing medium of the specified blog in the database.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("blogId", Type = typeof(Guid), Required = true, Description = "Id of the blog on which the medium exists", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(Medium), Required = true, Description = "Medium object to be updated")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Accepted, Description = "Accepted if the update operation succeeded")]
        [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "No medium with the specified id was found on this blog")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Response for unauthenticated requests.")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "Request cannot not be processed, see response body why")]
        [Function($"{nameof(MediumFunction)}_{nameof(Update)}")]
        public override async Task<HttpResponseData> Update([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = Route + "/{id}")] HttpRequestData req, string blogId, string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(blogId) || Guid.Parse(blogId) == default)
                    return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Required parameter 'blogId' (GUID) is not specified or cannot be parsed.");

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                Medium? mediumToUpdate = JsonConvert.DeserializeObject<Medium>(requestBody);

                if (mediumToUpdate != null)
                {
                    EntityModel.Medium? existingMedium =
                        await BlogContext.Media.
                                          Include(medium => medium.MediumType).
                                          SingleOrDefaultAsync(medium => medium.BlogId == Guid.Parse(blogId) &&
                                                                         medium.MediumId == Guid.Parse(id));

                    if (existingMedium == null)
                    {
                        Logger.LogWarning("Medium with Id {Id} not found", id);
                        return req.CreateResponse(HttpStatusCode.NotFound);
                    }

                    existingMedium.UpdateWith(mediumToUpdate);

                    await BlogContext.SaveChangesAsync();

                    return req.CreateResponse(HttpStatusCode.Accepted);

                }
                return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Submitted data is invalid, medium cannot be modified.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating medium with Id '{MediumId}' for blog with Id \'{BlogId}\'", id, blogId);
                return await req.CreateResponseDataAsync(HttpStatusCode.InternalServerError, "An internal server error occured. Error details logged.");
            }
        }


        [OpenApiOperation("DELETE", "Medium", Description = "Deletes an existing medium from the specified blog in the database.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("blogId", Type = typeof(Guid), Required = true, Description = "Id of the blog on which the medium exists", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "OK Response if succeeded")]
        [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "No medium with the specified id was found on this blog")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Response for unauthenticated requests.")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "Request cannot not be processed, see response body why")]
        [Function($"{nameof(MediumFunction)}_{nameof(Delete)}")]
        public override async Task<HttpResponseData> Delete([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = Route + "/{id}")] HttpRequestData req, string blogId, string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(blogId) || Guid.Parse(blogId) == default)
                    return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Required parameter 'blogId' (GUID) is not specified or cannot be parsed.");

                EntityModel.Medium? existingMedium =
                    await BlogContext.Media.
                                      SingleOrDefaultAsync(medium => medium.BlogId == Guid.Parse(blogId) &&
                                                                     medium.MediumId == Guid.Parse(id));

                if (existingMedium == null)
                {
                    Logger.LogWarning("Medium with Id {Id} not found", id);
                    return req.CreateResponse(HttpStatusCode.NotFound);
                }

                BlogContext.Media.Remove(existingMedium);

                await BlogContext.SaveChangesAsync();

                return req.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating medium with Id '{MediumId}' from blog with Id \'{BlogId}\'", id, blogId);
                return await req.CreateResponseDataAsync(HttpStatusCode.InternalServerError, "An internal server error occured. Error details logged.");
            }
        }

    }
}

