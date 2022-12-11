using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MSiccDev.ServerlessBlog.EFCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using MSiccDev.ServerlessBlog.ModelHelper;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MSiccDev.ServerlessBlog.EntityModel;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace MSiccDev.ServerlessBlog.BlogFunctions
{
    public class BlogFunction
    {
        private const string Route = "blog";
        private readonly ILogger _logger;
        private readonly BlogContext _blogContext;

        public BlogFunction(BlogContext blogContext, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<BlogFunction>();
            _blogContext = blogContext ?? throw new ArgumentNullException(nameof(blogContext));
        }

        [Function($"{nameof(BlogFunction)}_{nameof(Create)}")]
        public async Task<HttpResponseData> Create([HttpTrigger(AuthorizationLevel.Admin, new[] { "post" }, Route = Route)] HttpRequestData req)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                DtoModel.Blog? newBlog = JsonConvert.DeserializeObject<DtoModel.Blog>(requestBody);

                if (newBlog != null)
                {
                    //create just the empty blog
                    EntityEntry<EntityModel.Blog> createdBlog = _blogContext.Add(newBlog.CreateFrom());

                    await _blogContext.SaveChangesAsync();

                    return await req.CreateNewEntityCreatedResponseDataAsync(createdBlog.Entity.BlogId);
                }
                else
                {
                    return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Submitted data is invalid, blog cannot be created.");
                }
            }
            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, ex.ToString());
            }
        }

        [Function($"{nameof(BlogFunction)}_{nameof(Get)}")]
        public async Task<HttpResponseData> Get([HttpTrigger(AuthorizationLevel.Function, new[] { "get" }, Route = Route + "/{id?}")] HttpRequestData req, string? id = null)
        {
            try
            {
                Dictionary<string, string> queryParams = req.GetQueryParameterDictionary();

                bool includeDetails = false;
                if (queryParams.Any(p => p.Key == nameof(includeDetails).ToLowerInvariant()))
                    _ = bool.TryParse(queryParams[nameof(includeDetails).ToLowerInvariant()], out includeDetails);


                if (!string.IsNullOrWhiteSpace(id))
                {
                    EntityModel.Blog? existingBlog = null;

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
                        _logger.LogWarning($"Blog with Id {id} not found");
                        return req.CreateResponse(HttpStatusCode.NotFound);
                    }

                    return await req.CreateOKResponseDataWithJsonAsync(existingBlog.ToDto(includeDetails));

                }
                else
                {
                    _logger.LogInformation("Trying to get posts", queryParams.ToArray());

                    List<EntityModel.Blog> entityResultSet = new List<EntityModel.Blog>();

                    (int count, int skip) = req.GetPagingProperties();


                    entityResultSet = await _blogContext.Blogs.
                                          Skip(skip).
                                          Take(count).
                                          ToListAsync();


                    //always just return the plain blog list, details should be loaded per blog as they can be MASSIVE
                    List<DtoModel.Blog> resultSet = entityResultSet.Select(entity => entity.ToDto(false)).ToList();

                    return await req.CreateOKResponseDataWithJsonAsync(resultSet);
                }
            }

            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, ex.ToString());
            }
        }

        [Function($"{nameof(BlogFunction)}_{nameof(Update)}")]
        public async Task<HttpResponseData> Update([HttpTrigger(AuthorizationLevel.Function, new[] { "put" }, Route = Route + "/{id}")] HttpRequestData req, string id)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                DtoModel.Blog? updatedBlog = JsonConvert.DeserializeObject<DtoModel.Blog>(requestBody);

                if (updatedBlog != null)
                {
                    EntityModel.Blog? exisistingBlog =
                        await _blogContext.Blogs.SingleOrDefaultAsync(blog => blog.BlogId == Guid.Parse(id));

                    if (exisistingBlog != null)
                    {
                        exisistingBlog.UpdateWith(updatedBlog);

                        await _blogContext.SaveChangesAsync();
                        return req.CreateResponse(HttpStatusCode.Accepted);
                    }
                    else
                    {
                        _logger.LogWarning($"Blog with Id '{id}' not found.");
                        return req.CreateResponse(HttpStatusCode.NotFound);
                    }
                }
                else
                {
                    return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, "Submitted data is invalid, blog cannot be modified.");
                }
            }
            catch (Exception ex)
            {
                //TODO: better handling of these cases...
                return await req.CreateResponseDataAsync(HttpStatusCode.BadRequest, ex.ToString());
            }
        }


        [Function($"{nameof(BlogFunctions)}_{nameof(Delete)}")]
        public async Task<HttpResponseData> Delete([HttpTrigger(AuthorizationLevel.Admin, new[] { "delete" }, Route = Route + "/{id}")] HttpRequestData req, string id)
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
                    _logger.LogWarning($"Post with Id {id} not found");
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

