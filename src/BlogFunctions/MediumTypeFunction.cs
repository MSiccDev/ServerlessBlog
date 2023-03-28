using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MSiccDev.ServerlessBlog.DtoModel;
using MSiccDev.ServerlessBlog.EFCore;
using MSiccDev.ServerlessBlog.ModelHelper;
using Newtonsoft.Json;
namespace MSiccDev.ServerlessBlog.BlogFunctions
{
    public class MediumTypeFunction : BlogFunctionBase
    {
        private const string Route = "blog/{blogId}/mediumtype";

        public MediumTypeFunction(BlogContext blogContext, ILoggerFactory loggerFactory) : base(blogContext) =>
            Logger = loggerFactory.CreateLogger<MediumTypeFunction>();

        [OpenApiOperation("CREATE", "MediumType", Description = "Creates a new medium type for the specified blog in the database.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("blogId", Type = typeof(Guid), Required = true, Description = "Id of the blog the new medium type should live in")]
        [OpenApiRequestBody("application/json", typeof(MediumType), Required = true, Description = "MediumType object to be created")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Created, Description = "Created Response if succeeded")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Response for unauthenticated requests.")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "Request cannot not be processed, see response body why")]
        [Function($"{nameof(MediumTypeFunction)}_{nameof(Create)}")]
        public override async Task<HttpResponseData> Create([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Route)] HttpRequestData req, string blogId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(blogId) || Guid.Parse(blogId) == default)
                    return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Required parameter 'blogId' (GUID) is not specified or cannot be parsed.");

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                MediumType? mediumType = JsonConvert.DeserializeObject<MediumType>(requestBody);

                if (mediumType != null)
                {
                    EntityModel.MediumType newMediumType = mediumType.CreateFrom();

                    EntityEntry<EntityModel.MediumType> createdMediumType =
                        BlogContext.MediaTypes.Add(newMediumType);

                    await BlogContext.SaveChangesAsync();

                    return await req.CreateNewEntityCreatedResponseDataAsync(createdMediumType.Entity.MediumTypeId);


                }
                else
                {
                    return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest,"Submitted data is invalid, medium type cannot be created.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating medium type object on blog with Id {BlogId}", blogId);
                return await req.CreateResponseDataAsync(HttpStatusCode.InternalServerError, "An internal server error occured. Error details logged.");
            }
        }


        [OpenApiOperation("GET", "MediumType", Description = "Gets a list of medium types from the database.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("blogId", Type = typeof(Guid), Required = true, Description = "Id of the blog on which the medium type exists", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("skip", In = ParameterLocation.Query, Type = typeof(int), Required = true, Description = "skips the specified amount of entries from the results", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("count", In = ParameterLocation.Query, Type = typeof(int), Required = true, Description = "how many results are being returned per request", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(MediumType), Description = "Gets a list of medium types")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Response for unauthenticated requests.")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "Request cannot not be processed, see response body why")]
        [Function($"{nameof(MediumTypeFunction)}_{nameof(GetList)}")]
        public override async Task<HttpResponseData> GetList([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route)] HttpRequestData req, string blogId)
        {
            try
            {
                Logger.LogInformation("Trying to get MediaTypes...");
                
                if (string.IsNullOrWhiteSpace(blogId) || Guid.Parse(blogId) == default)
                    return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Required parameter 'blogId' (GUID) is not specified or cannot be parsed.");

                (int count, int skip) = req.GetPagingProperties();

                List<EntityModel.MediumType> entityResultSet = await BlogContext.MediaTypes.
                                                                                 Skip(skip).
                                                                                 Take(count).
                                                                                 ToListAsync();

                List<MediumType> resultSet = entityResultSet.Select(entity => entity.ToDto()).ToList();

                return await req.CreateOkResponseDataWithJsonAsync(resultSet, JsonSerializerSettings);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting medium type list for blog with Id \'{Id}\'", blogId);
                return await req.CreateResponseDataAsync(HttpStatusCode.InternalServerError, "An internal server error occured. Error details logged.");
            }
        }


        [OpenApiOperation("GET", "MediumType", Description = "Gets a medium type from the database.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("blogId", Type = typeof(Guid), Required = true, Description = "Id of the blog on which the medium type exists", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("id", Type = typeof(Guid), Required = false, Description = "Id of the desired medium type", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(MediumType), Description = "Gets a single medium type filtered by the specified Id")]
        [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "No medium type with the specified id was found on this blog")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Response for unauthenticated requests.")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "Request cannot not be processed, see response body why")]
        [Function($"{nameof(MediumTypeFunction)}_{nameof(GetSingle)}")]
        public override async Task<HttpResponseData> GetSingle([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route + "/{id}")] HttpRequestData req, string blogId, string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(blogId) || Guid.Parse(blogId) == default)
                    return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Required parameter 'blogId' (GUID) is not specified or cannot be parsed.");

                if (!string.IsNullOrWhiteSpace(id))
                {
                    EntityModel.MediumType? existingMediumType =
                        await BlogContext.MediaTypes.
                                          SingleOrDefaultAsync(mediumType => mediumType.MediumTypeId == Guid.Parse(id));

                    if (existingMediumType == null)
                    {
                        Logger.LogWarning("MediumType with Id {Id} not found", id);
                        return req.CreateResponse(HttpStatusCode.NotFound);
                    }

                    return await req.CreateOkResponseDataWithJsonAsync(existingMediumType.ToDto(), JsonSerializerSettings);
                }

                return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Submitted data is invalid, must specify BlogId");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting medium type with Id '{MediumTypeId}' for blog with Id \'{BlogId}\'", id, blogId);
                return await req.CreateResponseDataAsync(HttpStatusCode.InternalServerError, "An internal server error occured. Error details logged.");
            }
        }

        [OpenApiOperation("UPDATE", "MediumType", Description = "Updates an existing medium type of the specified blog in the database.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("blogId", Type = typeof(Guid), Required = true, Description = "Id of the blog on which the medium type exists", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(MediumType), Required = true, Description = "MediumType object to be updated")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Accepted, Description = "Accepted if the update operation succeeded")]
        [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "No medium type with the specified id was found on this blog")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Response for unauthenticated requests.")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "Request cannot not be processed, see response body why")]
        [Function($"{nameof(MediumTypeFunction)}_{nameof(Update)}")]
        public override async Task<HttpResponseData> Update([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = Route + "/{id}")] HttpRequestData req, string blogId, string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(blogId) || Guid.Parse(blogId) == default)
                    return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Required parameter 'blogId' (GUID) is not specified or cannot be parsed.");

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                MediumType? mediumTypeToUpdate = JsonConvert.DeserializeObject<MediumType>(requestBody);

                if (mediumTypeToUpdate != null)
                {
                    EntityModel.MediumType? existingMediumType =
                        await BlogContext.MediaTypes.
                                          SingleOrDefaultAsync(mediumtype => mediumtype.MediumTypeId == Guid.Parse(id));

                    if (existingMediumType == null)
                    {
                        Logger.LogWarning("MediumType with Id {Id} not found", id);
                        return req.CreateResponse(HttpStatusCode.NotFound);
                    }

                    existingMediumType.UpdateWith(mediumTypeToUpdate);

                    await BlogContext.SaveChangesAsync();

                    return req.CreateResponse(HttpStatusCode.Accepted);
                }
                else
                {

                    return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest,"Submitted data is invalid, MediumType cannot be modified.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating medium type with Id '{MediumTypeId}' for blog with Id \'{BlogId}\'", id, blogId);
                return await req.CreateResponseDataAsync(HttpStatusCode.InternalServerError, "An internal server error occured. Error details logged.");
            }
        }


        [OpenApiOperation("DELETE", "MediumType", Description = "Deletes an existing medium type from the specified blog in the database.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("blogId", Type = typeof(Guid), Required = true, Description = "Id of the blog on which the medium type exists", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "OK Response if succeeded")]
        [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "No medium type with the specified id was found on this blog")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Response for unauthenticated requests.")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "Request cannot not be processed, see response body why")]
        [Function($"{nameof(MediumTypeFunction)}_{nameof(Delete)}")]
        public override async Task<HttpResponseData> Delete([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = Route + "/{id}")] HttpRequestData req, string blogId, string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(blogId) || Guid.Parse(blogId) == default)
                    return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Required parameter 'blogId' (GUID) is not specified or cannot be parsed.");

                EntityModel.MediumType? existingMediumType =
                    await BlogContext.MediaTypes.
                                      SingleOrDefaultAsync(mediumType => mediumType.MediumTypeId == Guid.Parse(id));

                if (existingMediumType == null)
                {
                    Logger.LogWarning("MediumType with Id {Id} not found", id);
                    return req.CreateResponse(HttpStatusCode.NotFound);
                }

                BlogContext.MediaTypes.Remove(existingMediumType);

                await BlogContext.SaveChangesAsync();

                return req.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating medium type with Id '{MediumTypeId}' from blog with Id \'{BlogId}\'", id, blogId);
                return await req.CreateResponseDataAsync(HttpStatusCode.InternalServerError, "An internal server error occured. Error details logged.");
            }
        }

    }
}

