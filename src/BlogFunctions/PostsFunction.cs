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
    public class PostsFunction : BlogFunctionBase
    {
        private const string Route = "blog/{blogId}/post";

        public PostsFunction(BlogContext blogContext, ILoggerFactory loggerFactory) : base(blogContext) =>
            Logger = loggerFactory.CreateLogger<PostsFunction>();

        [OpenApiOperation("CREATE", "Post", Description = "Creates a new post for the specified blog in the database.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("blogId", Type = typeof(Guid), Required = true, Description = "Id of the blog the new post should live in")]
        [OpenApiRequestBody("application/json", typeof(Post), Required = true, Description = "Post object to be created")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Created, Description = "Created Response if succeeded")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Response for unauthenticated requests.")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "Request cannot not be processed, see response body why")]
        [Function($"{nameof(PostsFunction)}_{nameof(Create)}")]
        public override async Task<HttpResponseData> Create([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Route)] HttpRequestData req, string blogId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(blogId) || Guid.Parse(blogId) == default)
                    return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Required parameter 'blogId' (GUID) is not specified or cannot be parsed.");

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                Post? post = JsonConvert.DeserializeObject<Post>(requestBody);

                if (post != null)
                {
                    if (post.BlogId != Guid.Parse(blogId))
                        return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Cannot create a post on a different blog.");

                    EntityModel.Post newPostEntity = post.CreateFrom();

                    EntityEntry<EntityModel.Post> createdPost =
                        BlogContext.Posts.Add(newPostEntity);

                    await BlogContext.SaveChangesAsync();

                    return await req.CreateNewEntityCreatedResponseDataAsync(createdPost.Entity.PostId);

                }
                else
                {

                    return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Submitted data is invalid, post cannot be created.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating post object on blog with Id {BlogId}", blogId);
                return await req.CreateResponseDataAsync(HttpStatusCode.InternalServerError, "An internal server error occured. Error details logged.");
            }
        }


        [OpenApiOperation("GET", "Post", Description = "Gets a list of posts from the database.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("blogId", Type = typeof(Guid), Required = true, Description = "Id of the blog on which the posts exist", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("skip", Type = typeof(int), Required = true, Description = "skips the specified amount of entries from the results", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("count", Type = typeof(int), Required = true, Description = "how many results are being returned per request", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(Post), Description = "Gets a list of posts")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Response for unauthenticated requests.")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "Request cannot not be processed, see response body why")]
        [Function($"{nameof(PostsFunction)}_{nameof(GetList)}")]
        public override async Task<HttpResponseData> GetList([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route)] HttpRequestData req, string blogId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(blogId) || Guid.Parse(blogId) == default)
                    return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Required parameter 'blogId' (GUID) is not specified or cannot be parsed.");

                Logger.LogInformation("Trying to get posts");

                (int count, int skip) = req.GetPagingProperties();

                List<EntityModel.Post> entityResultSet = await BlogContext.Posts.
                                                                           Include(post => post.Tags).
                                                                           Include(post => post.PostTagMappings).
                                                                           Include(post => post.Author).
                                                                           Include(post => post.Media).
                                                                           ThenInclude(media => media.MediumType).
                                                                           Include(post => post.PostMediumMappings).
                                                                           Where(post => post.BlogId == Guid.Parse(blogId)).
                                                                           OrderByDescending(post => post.Published).
                                                                           Skip(skip).
                                                                           Take(count).ToListAsync();



                List<Post> resultSet = entityResultSet.Select(entity => entity.ToDto()).ToList();

                return await req.CreateOkResponseDataWithJsonAsync(resultSet, JsonSerializerSettings);
            }

            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting post list for blog with Id \'{Id}\'", blogId);
                return await req.CreateResponseDataAsync(HttpStatusCode.InternalServerError, "An internal server error occured. Error details logged.");
            }
        }




        [OpenApiOperation("GET", "Post", Description = "Gets a post the database.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("blogId", Type = typeof(Guid), Required = true, Description = "Id of the blog on which the post exists", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("id", Type = typeof(Guid), Required = false, Description = "Id of the desired post", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(Post), Description = "Gets a single post filtered by the specified Id")]
        [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "No post with the specified id was found on this blog")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Response for unauthenticated requests.")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "Request cannot not be processed, see response body why")]
        [Function($"{nameof(PostsFunction)}_{nameof(GetSingle)}")]
        public override async Task<HttpResponseData> GetSingle([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route + "/{id}")] HttpRequestData req, string blogId, string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(blogId) || Guid.Parse(blogId) == default)
                    return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Required parameter 'blogId' (GUID) is not specified or cannot be parsed.");

                if (!string.IsNullOrWhiteSpace(id))
                {
                    Logger.LogInformation("Trying to get post with Id {Id}...", id);

                    EntityModel.Post? existingPost = await BlogContext.Posts.
                                                                       Include(post => post.Tags).
                                                                       Include(post => post.PostTagMappings).
                                                                       Include(post => post.Author).
                                                                       Include(post => post.Media).
                                                                       ThenInclude(media => media.MediumType).
                                                                       Include(post => post.PostMediumMappings).
                                                                       SingleOrDefaultAsync(post => post.BlogId == Guid.Parse(blogId) &&
                                                                                                    post.PostId == Guid.Parse(id));


                    if (existingPost == null)
                    {
                        Logger.LogWarning("Post with Id {Id} not found", id);
                        return req.CreateResponse(HttpStatusCode.NotFound);
                    }

                    return await req.CreateOkResponseDataWithJsonAsync(existingPost.ToDto(), JsonSerializerSettings);
                }

                return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Submitted data is invalid, must specify BlogId");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting post with Id '{PostId}' for blog with Id \'{BlogId}\'", id, blogId);
                return await req.CreateResponseDataAsync(HttpStatusCode.InternalServerError, "An internal server error occured. Error details logged.");
            }
        }


        [OpenApiOperation("UPDATE", "Post", Description = "Updates an existing post of the specified blog in the database.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("blogId", Type = typeof(Guid), Required = true, Description = "Id of the blog on which the post exists", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(Post), Required = true, Description = "Post object to be updated")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Accepted, Description = "Accepted if the update operation succeeded")]
        [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "No post with the specified id was found on this blog")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Response for unauthenticated requests.")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "Request cannot not be processed, see response body why")]
        [Function($"{nameof(PostsFunction)}_{nameof(Update)}")]
        public override async Task<HttpResponseData> Update([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = Route + "/{id}")] HttpRequestData req, string blogId, string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(blogId) || Guid.Parse(blogId) == default)
                    return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Required parameter 'blogId' (GUID) is not specified or cannot be parsed.");

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                Post? postToUpdate = JsonConvert.DeserializeObject<Post>(requestBody);

                if (postToUpdate != null)
                {
                    EntityModel.Post? existingPost = await BlogContext.Posts.
                                                                       Include(post => post.Tags).
                                                                       Include(post => post.PostTagMappings).
                                                                       Include(post => post.Author).
                                                                       Include(post => post.Media).
                                                                       ThenInclude(media => media.MediumType).
                                                                       Include(post => post.PostMediumMappings).
                                                                       SingleOrDefaultAsync(post => post.BlogId == Guid.Parse(blogId) &&
                                                                                                    post.PostId == Guid.Parse(id));

                    if (existingPost == null)
                    {
                        Logger.LogWarning("Post with Id {Id} not found", id);
                        return req.CreateResponse(HttpStatusCode.NotFound);
                    }

                    DateTimeOffset lastUpdated = existingPost.LastModified;

                    existingPost.UpdateWith(postToUpdate);

                    if (existingPost.LastModified > lastUpdated)
                        await BlogContext.SaveChangesAsync();

                    return req.CreateResponse(HttpStatusCode.Accepted); 
                }
                else
                {
                    return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Submitted data is invalid, post cannot be modified.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating post with Id '{PostId}' for blog with Id \'{BlogId}\'", id, blogId);
                return await req.CreateResponseDataAsync(HttpStatusCode.InternalServerError, "An internal server error occured. Error details logged.");
            }
        }


        [OpenApiOperation("DELETE", "Post", Description = "Deletes an existing post from the specified blog in the database.", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter("blogId", Type = typeof(Guid), Required = true, Description = "Id of the blog on which the post exists", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "OK Response if succeeded")]
        [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "No post with the specified id was found on this blog")]
        [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Response for unauthenticated requests.")]
        [OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "Request cannot not be processed, see response body why")]
        [Function($"{nameof(PostsFunction)}_{nameof(Delete)}")]
        public override async Task<HttpResponseData> Delete([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = Route + "/{id}")] HttpRequestData req, string blogId, string id)
        {
            if (string.IsNullOrWhiteSpace(blogId) || Guid.Parse(blogId) == default)
                return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Required parameter 'blogId' (GUID) is not specified or cannot be parsed.");

            try
            {
                EntityModel.Post? existingPost = await BlogContext.Posts.
                                                                   Include(post => post.Tags).
                                                                   Include(post => post.Media).
                                                                   SingleOrDefaultAsync(post => post.BlogId == Guid.Parse(blogId) &&
                                                                                                post.PostId == Guid.Parse(id));

                if (existingPost == null)
                {
                    Logger.LogWarning("Post with Id {Id} not found", id);
                    return req.CreateResponse(HttpStatusCode.NotFound);
                }

                BlogContext.Posts.Remove(existingPost);
                await BlogContext.SaveChangesAsync();

                return req.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating post with Id '{PostId}' from blog with Id \'{BlogId}\'", id, blogId);
                return await req.CreateResponseDataAsync(HttpStatusCode.InternalServerError, "An internal server error occured. Error details logged.");
            }
        }

    }
}

