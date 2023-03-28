using System.Net;
using Azure.Core.Serialization;
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
using Newtonsoft.Json.Serialization;
namespace MSiccDev.ServerlessBlog.BlogFunctions
{
	public class BlogFunction
	{
		private const string Route = "blog";
		private readonly BlogContext _blogContext;
		private readonly ILogger _logger;

		private JsonSerializerSettings? _jsonSerializerSettings;

		public BlogFunction(BlogContext blogContext, ILoggerFactory loggerFactory)
		{
			_logger = loggerFactory.CreateLogger<BlogFunction>();
			_blogContext = blogContext ?? throw new ArgumentNullException(nameof(blogContext));

			CreateNewtonSoftSerializerSettings();
		}

		private void CreateNewtonSoftSerializerSettings()
		{
			_jsonSerializerSettings = NewtonsoftJsonObjectSerializer.CreateJsonSerializerSettings();

			_jsonSerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

			_jsonSerializerSettings.NullValueHandling = NullValueHandling.Ignore;
			_jsonSerializerSettings.Formatting = Formatting.Indented;
			_jsonSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
			_jsonSerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
			_jsonSerializerSettings.DateParseHandling = DateParseHandling.DateTimeOffset;
		}

		[OpenApiOperation("CREATE", "Blog", Description = "Creates a new blog in the database.", Visibility = OpenApiVisibilityType.Important)]
		[OpenApiRequestBody("application/json", typeof(Blog), Required = true, Description = "Blog object to be created")]
		[OpenApiResponseWithoutBody(HttpStatusCode.Created, Description = "Created Response if succeeded")]
		[OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Response for unauthenticated requests.")]
		[OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "Request cannot not be processed, see response body why")]
		[Function($"{nameof(BlogFunction)}_{nameof(Create)}")]
		public async Task<HttpResponseData> Create([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Route)] HttpRequestData req)
		{
			try
			{
				string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

				Blog? newBlog = JsonConvert.DeserializeObject<Blog>(requestBody);

				if (newBlog != null)
				{
					//create just the empty blog
					EntityEntry<EntityModel.Blog> createdBlog = _blogContext.Add(newBlog.CreateFrom());

					await _blogContext.SaveChangesAsync();

					return await req.CreateNewEntityCreatedResponseDataAsync(createdBlog.Entity.BlogId);
				}
				return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Submitted data is invalid, blog cannot be created.");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating blog object");
				return await req.CreateResponseDataAsync(HttpStatusCode.InternalServerError, "An internal server error occured. Error details logged.");
			}
		}


		[OpenApiOperation("GET", "Blog", Description = "Gets a list of blogs from the database.", Visibility = OpenApiVisibilityType.Important)]
		[OpenApiParameter("skip", In = ParameterLocation.Query, Type = typeof(int), Required = true, Description = "skips the specified amount of entries from the results", Visibility = OpenApiVisibilityType.Important)]
		[OpenApiParameter("count", In = ParameterLocation.Query, Type = typeof(int), Required = true, Description = "how many results are being returned per request", Visibility = OpenApiVisibilityType.Important)]
		[OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<BlogOverview>), Description = "Gets a list of blogs with children counts")]
		[OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Response for unauthenticated requests.")]
		[OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "Request cannot not be processed, see response body why")]
		[Function($"{nameof(BlogFunction)}_{nameof(GetBlogList)}")]
		public async Task<HttpResponseData> GetBlogList([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route)] HttpRequestData req, string? id = null)
		{
			try
			{
				_logger.LogInformation("Trying to get blogs");

				List<BlogOverview> resultSet = new List<BlogOverview>();
				(int count, int skip) = req.GetPagingProperties();

				List<EntityModel.Blog> entityResultSet = await _blogContext.Blogs.
				                                                            Skip(skip).
				                                                            Take(count).
				                                                            ToListAsync();

				foreach (EntityModel.Blog result in entityResultSet)
				{
					int authorCount = await _blogContext.Authors.CountAsync(author => author.BlogId == result.BlogId);
					int mediaCount = await _blogContext.Media.CountAsync(medium => medium.BlogId == result.BlogId);
					int tagsCount = await _blogContext.Tags.CountAsync(tag => tag.BlogId == result.BlogId);
					int postsCount = await _blogContext.Posts.CountAsync(post => post.BlogId == result.BlogId);

					BlogOverview overViewResult = new BlogOverview(result.ToDto(false), authorCount, mediaCount, tagsCount, postsCount);
					resultSet.Add(overViewResult);
				}

				return await req.CreateOkResponseDataWithJsonAsync(resultSet, _jsonSerializerSettings);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting blog list");
				return await req.CreateResponseDataAsync(HttpStatusCode.InternalServerError, "An internal server error occured. Error details logged.");
			}
		}


		[OpenApiOperation("GET", "Blog", Description = "Gets a blog by its id from the database.", Visibility = OpenApiVisibilityType.Important)]
		[OpenApiParameter("id", Type = typeof(Guid), Required = true, Description = "Id of the desired blog", Visibility = OpenApiVisibilityType.Important)]
		[OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(Blog), Description = "Gets a single blog filtered by the specified Id. Warning: This can result in a very big json!")]
		[OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Response for unauthenticated requests.")]
		[OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "No blog with the specified id was found")]
		[OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "Request cannot not be processed, see response body why")]
		[Function($"{nameof(BlogFunction)}_{nameof(GetBlog)}")]
		public async Task<HttpResponseData> GetBlog([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route + "/{id}")] HttpRequestData req, string? id = null)
		{
			try
			{
				if (!string.IsNullOrWhiteSpace(id))
				{
					EntityModel.Blog? existingBlog = await _blogContext.Blogs.
					                                                    Include(blog => blog.Authors).
					                                                    ThenInclude(author => author.UserImage).
					                                                    ThenInclude(medium => medium.MediumType).
					                                                    Include(blog => blog.Media).
					                                                    ThenInclude(medium => medium.MediumType).
					                                                    Include(blog => blog.Tags).
					                                                    Include(blog => blog.Posts).
					                                                    ThenInclude(post => post.Author).
					                                                    ThenInclude(author => author.UserImage).
					                                                    ThenInclude(medium => medium.MediumType).
					                                                    Include(blog => blog.Posts).
					                                                    ThenInclude(post => post.Media).
					                                                    ThenInclude(media => media.MediumType).
					                                                    Include(blog => blog.Posts).
					                                                    ThenInclude(post => post.Tags).
					                                                    Include(blog => blog.Posts).
					                                                    ThenInclude(post => post.PostTagMappings).
					                                                    Include(blog => blog.Posts).
					                                                    ThenInclude(post => post.PostMediumMappings).
					                                                    SingleOrDefaultAsync(blog => blog.BlogId == Guid.Parse(id));


					if (existingBlog == null)
					{
						_logger.LogWarning("Blog with Id \'{Id}\' not found", id);
						return req.CreateResponse(HttpStatusCode.NotFound);
					}

					return await req.CreateOkResponseDataWithJsonAsync(existingBlog.ToDto(), _jsonSerializerSettings);
				}

				return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Submitted data is invalid, must specify BlogId");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting blog object with Id \'{Id}\'", id);
				return await req.CreateResponseDataAsync(HttpStatusCode.InternalServerError, "An internal server error occured. Error details logged.");
			}
		}


		[OpenApiOperation("UPDATE", "Blog", Description = "Updates a blog in the database.", Visibility = OpenApiVisibilityType.Important)]
		[OpenApiRequestBody("application/json", typeof(Blog), Required = true, Description = "Blog object to be updated")]
		[OpenApiParameter("id", Type = typeof(Guid?), Required = true, Description = "Id of the blog to update", Visibility = OpenApiVisibilityType.Important)]
		[OpenApiResponseWithoutBody(HttpStatusCode.Accepted, Description = "Accepted if the update operation succeeded")]
		[OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "No blog with the specified id was found")]
		[OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Response for unauthenticated requests.")]
		[OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "Request cannot not be processed, see response body why")]
		[Function($"{nameof(BlogFunction)}_{nameof(Update)}")]
		public async Task<HttpResponseData> Update([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = Route + "/{id}")] HttpRequestData req, string id)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(id) || Guid.Parse(id) == default)
					return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Required parameter 'id' (GUID) is not specified or cannot be parsed.");

				string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

				Blog? updatedBlog = JsonConvert.DeserializeObject<Blog>(requestBody);

				if (updatedBlog != null)
				{
					EntityModel.Blog? existingBlog =
						await _blogContext.Blogs.SingleOrDefaultAsync(blog => blog.BlogId == Guid.Parse(id));

					if (existingBlog != null)
					{
						existingBlog.UpdateWith(updatedBlog);

						await _blogContext.SaveChangesAsync();
						return req.CreateResponse(HttpStatusCode.Accepted);
					}
					_logger.LogWarning("Blog with Id \'{Id}\' not found", id);
					return req.CreateResponse(HttpStatusCode.NotFound);
				}
				return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Submitted data is invalid, blog cannot be modified.");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating blog with Id \'{Id}\'", id);
				return await req.CreateResponseDataAsync(HttpStatusCode.InternalServerError, "An internal server error occured. Error details logged.");
			}
		}

		[OpenApiOperation("DELETE", "Blog", Description = "Delete a blog including all sub-entities from the database.", Visibility = OpenApiVisibilityType.Important)]
		[OpenApiParameter("id", Type = typeof(Guid?), Required = true, Description = "Id of the blog to delete", Visibility = OpenApiVisibilityType.Important)]
		[OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "OK response if the delete operation succeeded")]
		[OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "No blog with the specified id was found")]
		[OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized, Description = "Response for unauthenticated requests.")]
		[OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "Request cannot not be processed, see response body why")]
		[Function($"{nameof(BlogFunctions)}_{nameof(Delete)}")]
		public async Task<HttpResponseData> Delete([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = Route + "/{id}")] HttpRequestData req, string id)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(id) || Guid.Parse(id) == default)
					return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Required parameter 'id' (GUID) is not specified or cannot be parsed.");

				EntityModel.Blog? existingBlog = await _blogContext.Blogs.
				                                                    Include(blog => blog.Authors).
				                                                    ThenInclude(author => author.UserImage).
				                                                    ThenInclude(medium => medium.MediumType).
				                                                    Include(blog => blog.Media).
				                                                    ThenInclude(medium => medium.MediumType).
				                                                    Include(blog => blog.Tags).
				                                                    Include(blog => blog.Posts).
				                                                    ThenInclude(post => post.Author).
				                                                    ThenInclude(author => author.UserImage).
				                                                    ThenInclude(medium => medium.MediumType).
				                                                    Include(blog => blog.Posts).
				                                                    ThenInclude(post => post.Media).
				                                                    ThenInclude(media => media.MediumType).
				                                                    Include(blog => blog.Posts).
				                                                    ThenInclude(post => post.Tags).
				                                                    Include(blog => blog.Posts).
				                                                    ThenInclude(post => post.PostTagMappings).
				                                                    Include(blog => blog.Posts).
				                                                    ThenInclude(post => post.PostMediumMappings).
				                                                    SingleOrDefaultAsync(blog => blog.BlogId == Guid.Parse(id));

				if (existingBlog == null)
				{
					_logger.LogWarning("Blog with Id \'{Id}\' not found", id);
					return req.CreateResponse(HttpStatusCode.NotFound);
				}

				_blogContext.Blogs.Remove(existingBlog);
				await _blogContext.SaveChangesAsync();

				return req.CreateResponse(HttpStatusCode.OK);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error deleting blog with Id \'{Id}\'", id);
				return await req.CreateResponseDataAsync(HttpStatusCode.InternalServerError, "An internal server error occured. Error details logged.");
			}
		}

	}
}
