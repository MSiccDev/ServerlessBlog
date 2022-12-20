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
		[OpenApiSecurity("ApiKey", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Header)]
		[OpenApiRequestBody("application/json", typeof(Blog), Required = true, Description = "Blog object to be created")]
		[OpenApiResponseWithoutBody(HttpStatusCode.Created, Description = "Created Response if succeeded")]
		[OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "Request cannot not be processed, see response body why")]
		[Function($"{nameof(BlogFunction)}_{nameof(Create)}")]
		public async Task<HttpResponseData> Create([HttpTrigger(AuthorizationLevel.Admin, "post", Route = Route)] HttpRequestData req)
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
				//TODO: better handling of these cases...
				return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, ex.ToString());
			}
		}


		[OpenApiOperation("GET", "Blog", Description = "Gets a list of blogs from the database.", Visibility = OpenApiVisibilityType.Important)]
		[OpenApiSecurity("ApiKey", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Header)]
		[OpenApiParameter("skip", In = ParameterLocation.Query, Type = typeof(int), Required = true, Description = "skips the specified amount of entries from the results", Visibility = OpenApiVisibilityType.Important)]
		[OpenApiParameter("count", In = ParameterLocation.Query, Type = typeof(int), Required = true, Description = "how many results are being returned per request", Visibility = OpenApiVisibilityType.Important)]
		[OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<Blog>), Description = "Gets a list of blogs when no Id is specified or a single blog filtered by the specified Id")]
		[OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "Request cannot not be processed, see response body why")]
		[Function($"{nameof(BlogFunction)}_{nameof(GetBlogList)}")]
		public async Task<HttpResponseData> GetBlogList([HttpTrigger(AuthorizationLevel.Function, "get", Route = Route)] HttpRequestData req, string? id = null)
		{
			try
			{
				_logger.LogInformation("Trying to get blogs");

				(int count, int skip) = req.GetPagingProperties();

				List<EntityModel.Blog> entityResultSet = await _blogContext.Blogs.
				                                                            Skip(skip).
				                                                            Take(count).
				                                                            ToListAsync();


				//always just return the plain blog list, details should be loaded per blog as they can be MASSIVE
				List<Blog> resultSet = entityResultSet.Select(entity => entity.ToDto(false)).
				                                       ToList();

				return await req.CreateOkResponseDataWithJsonAsync(resultSet, _jsonSerializerSettings);
			}
			catch (Exception ex)
			{
				//TODO: better handling of these cases...
				return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, ex.ToString());
			}
		}


		[OpenApiOperation("GET", "Blog", Description = "Gets a blog by its id from the database.", Visibility = OpenApiVisibilityType.Important)]
		[OpenApiSecurity("ApiKey", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Header)]
		[OpenApiParameter("id", In = ParameterLocation.Path, Type = typeof(Guid), Required = true, Description = "Id of the desired blog", Visibility = OpenApiVisibilityType.Important)]
		[OpenApiParameter("includeDetails", In = ParameterLocation.Query, Required = true, Type = typeof(bool), Description = "Control if response should contain Posts, Tags, Media and Authors. Warning: This could result in a very large .json. Treat this as full export.", Visibility = OpenApiVisibilityType.Important)]
		[OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(Blog), Description = "Gets a single blog filtered by the specified Id")]
		[OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "No blog with the specified id was found")]
		[OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "Request cannot not be processed, see response body why")]
		[Function($"{nameof(BlogFunction)}_{nameof(GetBlog)}")]
		public async Task<HttpResponseData> GetBlog([HttpTrigger(AuthorizationLevel.Function, "get", Route = Route + "/{id}")] HttpRequestData req, string? id = null)
		{
			try
			{
				if (!string.IsNullOrWhiteSpace(id))
				{
					EntityModel.Blog? existingBlog;
					bool includeDetails = req.IncludeDetails();

					if (includeDetails)
					{
						existingBlog = await _blogContext.Blogs.
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
					}
					else
					{
						existingBlog = await _blogContext.Blogs.SingleOrDefaultAsync(blog => blog.BlogId == Guid.Parse(id));
					}

					if (existingBlog == null)
					{
						_logger.LogWarning("Blog with Id {Id} not found", id);
						return req.CreateResponse(HttpStatusCode.NotFound);
					}

					return await req.CreateOkResponseDataWithJsonAsync(existingBlog.ToDto(includeDetails), _jsonSerializerSettings);
				}

				return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Submitted data is invalid, must specify BlogId");
			}
			catch (Exception ex)
			{
				//TODO: better handling of these cases...
				return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, ex.ToString());
			}
		}


		[OpenApiOperation("UPDATE", "Blog", Description = "Updates a blog in the database.", Visibility = OpenApiVisibilityType.Important)]
		[OpenApiSecurity("ApiKey", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Header)]
		[OpenApiRequestBody("application/json", typeof(Blog), Required = true, Description = "Blog object to be updated")]
		[OpenApiParameter("id", In = ParameterLocation.Path, Type = typeof(Guid?), Required = true, Description = "Id of the blog to update", Visibility = OpenApiVisibilityType.Important)]
		[OpenApiResponseWithoutBody(HttpStatusCode.Accepted, Description = "Accepted if the update operation succeeded")]
		[OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "No blog with the specified id was found")]
		[OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "Request cannot not be processed, see response body why")]
		[Function($"{nameof(BlogFunction)}_{nameof(Update)}")]
		public async Task<HttpResponseData> Update([HttpTrigger(AuthorizationLevel.Function, "put", Route = Route + "/{id}")] HttpRequestData req, string id)
		{
			try
			{
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
				//TODO: better handling of these cases...
				return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, ex.ToString());
			}
		}

		[OpenApiOperation("DELETE", "Blog", Description = "Delete a blog including all sub-entities from the database.", Visibility = OpenApiVisibilityType.Important)]
		[OpenApiSecurity("ApiKey", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Header)]
		[OpenApiParameter("id", In = ParameterLocation.Path, Type = typeof(Guid?), Required = true, Description = "Id of the blog to delete", Visibility = OpenApiVisibilityType.Important)]
		[OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "OK response if the delete operation succeeded")]
		[OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "No blog with the specified id was found")]
		[OpenApiResponseWithBody(HttpStatusCode.BadRequest, "text/plain", typeof(string), Description = "Request cannot not be processed, see response body why")]
		[Function($"{nameof(BlogFunctions)}_{nameof(Delete)}")]
		public async Task<HttpResponseData> Delete([HttpTrigger(AuthorizationLevel.Admin, "delete", Route = Route + "/{id}")] HttpRequestData req, string id)
		{
			try
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
					_logger.LogWarning("Post with Id {Id} not found", id);
					return req.CreateResponse(HttpStatusCode.NotFound);
				}

				_blogContext.Blogs.Remove(existingBlog);
				await _blogContext.SaveChangesAsync();

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
