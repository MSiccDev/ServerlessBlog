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
    public class AuthorFunction : BlogFunctionBase
    {
        private const string Route = "blog/{blogId}/author";

        public AuthorFunction(BlogContext blogContext, ILoggerFactory loggerFactory) : base(blogContext)
        {
            Logger = loggerFactory.CreateLogger<AuthorFunction>();
        }

        [OpenApiOperation("CREATE", "Author", Description = "Creates a new author for the specified blog in the database.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("blogId", Type = typeof(Guid), Required = true, Description = "Id of the blog the new author should live in")]
        [OpenApiRequestBody("application/json", typeof(Author), Required = true, Description = "Author object to be created")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Created, Description = "Created Response if succeeded")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Response for unauthenticated requests.")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "Request cannot not be processed, see response body why")]
        [Function($"{nameof(AuthorFunction)}_{nameof(Create)}")]
        public override async Task<HttpResponseData> Create([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Route)] HttpRequestData req, string blogId)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                Author? author = JsonConvert.DeserializeObject<Author>(requestBody);

                if (author != null)
                {

                    EntityModel.Author? newAuthorEntity = author.CreateFrom(Guid.Parse(blogId));

                    EntityEntry<EntityModel.Author> createdAuthor =
                        BlogContext.Authors.Add(newAuthorEntity);

                    await BlogContext.SaveChangesAsync();

                    return await req.CreateNewEntityCreatedResponseDataAsync(createdAuthor.Entity.AuthorId);
                }
                else
                {
                    return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Submitted data is invalid, author cannot be created.");
                }
            }
            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, ex.ToString());
            }
        }



        [OpenApiOperation("GET", "Author", Description = "Gets a list of authors from the database.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("blogId", Type = typeof(Guid?), Required = true, Description = "Id of the blog on which the authors exist", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("skip", Type = typeof(int), Required = true, Description = "skips the specified amount of entries from the results", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("count", Type = typeof(int), Required = true, Description = "how many results are being returned per request", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(Author), Description = "Returns a list of authors from the Database")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Response for unauthenticated requests.")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "Request cannot not be processed, see response body why")]
        [Function($"{nameof(AuthorFunction)}_{nameof(GetList)}")]
        public override async Task<HttpResponseData> GetList([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route)] HttpRequestData req, string blogId)
        {
            try
            {
                Logger.LogInformation("Trying to get authors...");

                if (string.IsNullOrWhiteSpace(blogId) || Guid.Parse(blogId) == default)
                    return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Required parameter 'blogId' (GUID) is not specified or cannot be parsed.");

                (int count, int skip) = req.GetPagingProperties();

                List<EntityModel.Author> entityResultSet = await BlogContext.Authors.
                                                                             Where(author => author.BlogId == Guid.Parse(blogId)).
                                                                             Skip(skip).
                                                                             Take(count).
                                                                             ToListAsync();

                List<Author> resultSet = entityResultSet.Select(entity => entity.ToDto()).ToList();

                return await req.CreateOkResponseDataWithJsonAsync(resultSet, JsonSerializerSettings);
            }
            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, ex.ToString());
            }
        }


        [OpenApiOperation("GET", "Author", Description = "Gets an author from the database.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("blogId", Type = typeof(Guid?), Required = true, Description = "Id of the blog on which the author exists", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("id", Type = typeof(Guid?), Required = false, Description = "Id of the desired author", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(Author), Description = "Gets a single author filtered by the specified Id")]
        [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "No author with the specified id was found on this blog")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Response for unauthenticated requests.")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "Request cannot not be processed, see response body why")]
        [Function($"{nameof(AuthorFunction)}_{nameof(GetSingle)}")]
        public override async Task<HttpResponseData> GetSingle([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route + "/{id}")] HttpRequestData req, string blogId, string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(blogId) || Guid.Parse(blogId) == default)
                    return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Required parameter 'blogId' (GUID) is not specified or cannot be parsed.");
                
                if (!string.IsNullOrWhiteSpace(id))
                {
                    Logger.LogInformation("Trying to get author with Id: {Id}...", id);

                    EntityModel.Author? existingAuthor =
                        await BlogContext.Authors.
                                          Include(author => author.UserImage).
                                          ThenInclude(media => media.MediumType).
                                          SingleOrDefaultAsync(author => author.BlogId == Guid.Parse(blogId) &&
                                                                         author.AuthorId == Guid.Parse(id));

                    if (existingAuthor == null)
                    {
                        Logger.LogWarning("Author with Id {Id} not found", id);
                        return req.CreateResponse(HttpStatusCode.NotFound);
                    }

                    return await req.CreateOkResponseDataWithJsonAsync(existingAuthor.ToDto(), JsonSerializerSettings);
                }
                else
                {
                    return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Submitted data is invalid, must specify BlogId");
                }
            }
            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, ex.ToString());
            }
        }


        [OpenApiOperation("UPDATE", "Author", Description = "Updates an existing author of the specified blog in the database.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("blogId", Type = typeof(Guid?), Required = true, Description = "Id of the blog on which the author exists", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(Author), Required = true, Description = "Author object to be updated")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Accepted, Description = "Accepted if the update operation succeeded")]
        [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "No author with the specified id was found on this blog")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Response for unauthenticated requests.")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "Request cannot not be processed, see response body why")]
        [Function($"{nameof(AuthorFunction)}_{nameof(Update)}")]
        public override async Task<HttpResponseData> Update([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = Route + "/{id}")] HttpRequestData req, string blogId, string id)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                Author? authorToUpdate = JsonConvert.DeserializeObject<Author>(requestBody);

                if (authorToUpdate != null)
                {
                    EntityModel.Author? existingAuthor =
                        await BlogContext.Authors.
                                          Include(author => author.UserImage).
                                          ThenInclude(media => media.MediumType).
                                          SingleOrDefaultAsync(author => author.BlogId == Guid.Parse(blogId) &&
                                                                         author.AuthorId == Guid.Parse(id));

                    if (existingAuthor == null)
                    {
                        Logger.LogWarning("Author with Id {Id} not found", id);
                        return req.CreateResponse(HttpStatusCode.NotFound);
                    }

                    existingAuthor.UpdateWith(authorToUpdate);

                    await BlogContext.SaveChangesAsync();

                    return req.CreateResponse(HttpStatusCode.Accepted);
                }
                else
                {
                    return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Submitted data is invalid, author cannot be modified.");
                }
            }
            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, ex.ToString());
            }
        }


        [OpenApiOperation("DELETE", "Author", Description = "Deletes an existing author from the specified blog in the database.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("blogId", Type = typeof(Guid), Required = true, Description = "Id of the blog on which the author exists", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("id", Type = typeof(Guid?), Required = false, Description = "Id of the author to delete", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "OK Response if succeeded")]
        [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "No author with the specified id was found on this blog")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Response for unauthenticated requests.")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "Request cannot not be processed, see response body why")]
        [Function($"{nameof(AuthorFunction)}_{nameof(Delete)}")]
        public override async Task<HttpResponseData> Delete([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = Route + "/{id}")] HttpRequestData req, string blogId, string id)
        {
            try
            {
                EntityModel.Author? existingAuthor = await BlogContext.Authors.
                                                                       Include(author => author.UserImage).
                                                                       SingleOrDefaultAsync(author => author.BlogId == Guid.Parse(blogId) &&
                                                                                                      author.AuthorId == Guid.Parse(id));

                if (existingAuthor == null)
                {
                    Logger.LogWarning("Author with Id {Id} not found", id);
                    return req.CreateResponse(HttpStatusCode.NotFound);
                }

                BlogContext.Authors.Remove(existingAuthor);

                await BlogContext.SaveChangesAsync();

                return req.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, ex.ToString());
            }
        }


    }
}

